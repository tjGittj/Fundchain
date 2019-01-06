using Microsoft.Extensions.Options;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Table;
using Nethereum.Web3;
using System;
using System.Threading.Tasks;
using EthereumStart.Models;
using Nethereum.Contracts;
using Nethereum.Geth;
using System.Numerics;
using System.Threading;
using Nethereum.ABI.FunctionEncoding.Attributes;

namespace EthereumStart.Services
{
    public class BasicEthereumService : IEthereumService
    {
        private Nethereum.Web3.Web3 _web3;
        private string _accountAddress;
        private string _password;
        private string _storageKey;
        private string _storageAccount;

        public string AccountAddress
        {
            get
            {
                return _accountAddress;
            }

            set
            {
                _accountAddress = value;
            }
        }

        public BasicEthereumService(IOptions<EthereumSettings> config)
        {
            //_web3 = new Web3("http://127.0.0.1:8000");
            _web3 = new Web3("http://52.187.147.215:8000");
            _accountAddress = config.Value.EhtereumAccount;
            _password = config.Value.EhtereumPassword;
            _storageAccount = config.Value.StorageAccount;
            _storageKey = config.Value.StorageKey;
            _web3.TransactionManager.DefaultGas = BigInteger.Parse("290000");
            _web3.TransactionManager.DefaultGasPrice = BigInteger.Parse("1");
        }

        public async Task<bool> SaveContractToTableStorage(EthereumContractInfo contract)
        {
            StorageCredentials credentials = new StorageCredentials(_storageAccount, _storageKey);
            CloudStorageAccount account = new CloudStorageAccount(credentials, true);
            var client = account.CreateCloudTableClient();

            var tableRef = client.GetTableReference("ethtransactions");
            await tableRef.CreateIfNotExistsAsync();

            TableOperation ops = TableOperation.InsertOrMerge(contract);
            await tableRef.ExecuteAsync(ops);
            return true;
        }

        public async Task<EthereumContractInfo> GetContractFromTableStorage(string name)
        {
            StorageCredentials credentials = new StorageCredentials(_storageAccount, _storageKey);
            CloudStorageAccount account = new CloudStorageAccount(credentials, true);
            var client = account.CreateCloudTableClient();

            var tableRef = client.GetTableReference("ethtransactions");
            await tableRef.CreateIfNotExistsAsync();

            TableOperation ops = TableOperation.Retrieve<EthereumContractInfo>("contract", name);
            var tableResult = await tableRef.ExecuteAsync(ops);
            if (tableResult.HttpStatusCode == 200)
                return (EthereumContractInfo)tableResult.Result;
            else
                return null;
        }

        public async Task<decimal> GetBalance(string address)
        {
            var balance = await _web3.Eth.GetBalance.SendRequestAsync(address);
            return Nethereum.Util.UnitConversion.Convert.FromWei(balance.Value, 18);
        }

        public async Task<bool> TransferEthers(string addressFrom, string addressTo, int ethers)
        {
            var unlockResult1 = await UnlockAccount(addressFrom);
            var unlockResult2 = await UnlockAccount(addressTo);
            if (unlockResult1 && unlockResult2)
            {
                var weiValue = Nethereum.Util.UnitConversion.Convert.ToWei(ethers);
                var transactionHash = await _web3.TransactionManager.SendTransactionAsync(addressFrom, addressTo, new Nethereum.Hex.HexTypes.HexBigInteger(weiValue));
                await MineAndGetReceiptAsync(transactionHash);
                return true;
            }
            return false;
        }

        public async Task<bool> ReleaseContract(string name, string abi, string byteCode, int gas)
        {

            // check contractName
            var existing = await this.GetContractFromTableStorage(name);
            if (existing != null) throw new Exception($"Contract {name} is present in storage");
            try
            {
                var resultUnlocking = await _web3.Personal.UnlockAccount.SendRequestAsync(_accountAddress, _password, 60);
                if (resultUnlocking)
                {
                    var transactionHash = await _web3.Eth.DeployContract.SendRequestAsync(abi, byteCode, _accountAddress, new Nethereum.Hex.HexTypes.HexBigInteger(gas));

                    EthereumContractInfo eci = new EthereumContractInfo(name, abi, byteCode, transactionHash);
                    return await SaveContractToTableStorage(eci);
                }
            }
            catch (Exception exc)
            {
                return false;
            }
            return false;
        }

        public async Task<string> TryGetContractAddress(string name)
        {
            // check contractName
            var existing = await this.GetContractFromTableStorage(name);
            if (existing == null) throw new Exception($"Contract {name} does not exist in storage");

            if (!String.IsNullOrEmpty(existing.ContractAddress))
                return existing.ContractAddress;
            else
            {
                var resultUnlocking = await _web3.Personal.UnlockAccount.SendRequestAsync(_accountAddress, _password, 60);
                if (resultUnlocking)
                {
                    var receipt = await _web3.Eth.Transactions.GetTransactionReceipt.SendRequestAsync(existing.TransactionHash);
                    if (receipt != null)
                    {
                        existing.ContractAddress = receipt.ContractAddress;
                        await SaveContractToTableStorage(existing);
                        return existing.ContractAddress;
                    }
                }
            }
            return null;
        }

        public async Task<Contract> GetContract(string name)
        {
            //var existing = await this.GetContractFromTableStorage(name);
            //if (existing == null) throw new Exception($"Contract {name} does not exist in storage");
            //if (existing.ContractAddress == null) throw new Exception($"Contract address for {name} is empty. Please call TryGetContractAddress until it returns the address");

            //var resultUnlocking = await _web3.Personal.UnlockAccount.SendRequestAsync(_accountAddress, _password, 60);
            //if (resultUnlocking)
            //{
            //    return _web3.Eth.GetContract(existing.Abi, existing.ContractAddress);
            //}
            //return null;
            //string abi = @"[ { 'constant': true, 'inputs': [ { 'name': '', 'type': 'address' } ], 'name': 'currentCount', 'outputs': [ { 'name': '', 'type': 'uint256', 'value': '0' } ], 'payable': false, 'stateMutability': 'view', 'type': 'function' }, { 'constant': false, 'inputs': [ { 'name': 'accountId', 'type': 'address' } ], 'name': 'login', 'outputs': [ { 'name': '', 'type': 'bool' } ], 'payable': false, 'stateMutability': 'nonpayable', 'type': 'function' }, { 'constant': true, 'inputs': [ { 'name': '', 'type': 'address' } ], 'name': 'maxCount', 'outputs': [ { 'name': '', 'type': 'uint256', 'value': '0' } ], 'payable': false, 'stateMutability': 'view', 'type': 'function' }, { 'constant': false, 'inputs': [ { 'name': 'accountId', 'type': 'address' }, { 'name': 'count', 'type': 'uint256' } ], 'name': 'setConcurrency', 'outputs': [ { 'name': '', 'type': 'bool' } ], 'payable': false, 'stateMutability': 'nonpayable', 'type': 'function' }, { 'constant': false, 'inputs': [ { 'name': 'accountId', 'type': 'address' } ], 'name': 'getConcurrencyBalance', 'outputs': [ { 'name': '', 'type': 'uint256' } ], 'payable': false, 'stateMutability': 'nonpayable', 'type': 'function' }, { 'constant': false, 'inputs': [ { 'name': 'accountId', 'type': 'address' } ], 'name': 'logout', 'outputs': [ { 'name': '', 'type': 'bool' } ], 'payable': false, 'stateMutability': 'nonpayable', 'type': 'function' }, { 'anonymous': false, 'inputs': [ { 'indexed': false, 'name': 'currentCount', 'type': 'uint256' }, { 'indexed': true, 'name': 'user', 'type': 'address' }, { 'indexed': false, 'name': 'action', 'type': 'string' }, { 'indexed': false, 'name': 'result', 'type': 'bool' } ], 'name': 'Transaction', 'type': 'event' } ]";
            string abi = @"[ { 'constant': true, 'inputs': [ { 'name': '', 'type': 'address' } ], 'name': 'currentCount', 'outputs': [ { 'name': '', 'type': 'uint256', 'value': '0' } ], 'payable': false, 'stateMutability': 'view', 'type': 'function' }, { 'constant': false, 'inputs': [ { 'name': 'accountId', 'type': 'address' }, { 'name': 'count', 'type': 'uint256' }, { 'name': 'currentuser', 'type': 'string' } ], 'name': 'setConcurrency', 'outputs': [ { 'name': '', 'type': 'bool' } ], 'payable': false, 'stateMutability': 'nonpayable', 'type': 'function' }, { 'constant': false, 'inputs': [ { 'name': 'accountId', 'type': 'address' }, { 'name': 'currentuser', 'type': 'string' } ], 'name': 'logout', 'outputs': [ { 'name': '', 'type': 'bool' } ], 'payable': false, 'stateMutability': 'nonpayable', 'type': 'function' }, { 'constant': false, 'inputs': [ { 'name': 'accountId', 'type': 'address' }, { 'name': 'currentuser', 'type': 'string' } ], 'name': 'login', 'outputs': [ { 'name': '', 'type': 'bool' } ], 'payable': false, 'stateMutability': 'nonpayable', 'type': 'function' }, { 'constant': true, 'inputs': [ { 'name': '', 'type': 'address' } ], 'name': 'maxCount', 'outputs': [ { 'name': '', 'type': 'uint256', 'value': '0' } ], 'payable': false, 'stateMutability': 'view', 'type': 'function' }, { 'constant': false, 'inputs': [ { 'name': 'accountId', 'type': 'address' }, { 'name': 'count', 'type': 'uint256' }, { 'name': 'currentuser', 'type': 'string' } ], 'name': 'incrementConcurrency', 'outputs': [ { 'name': '', 'type': 'bool' } ], 'payable': false, 'stateMutability': 'nonpayable', 'type': 'function' }, { 'constant': false, 'inputs': [ { 'name': 'accountId', 'type': 'address' } ], 'name': 'getConcurrencyBalance', 'outputs': [ { 'name': '', 'type': 'uint256' } ], 'payable': false, 'stateMutability': 'nonpayable', 'type': 'function' }, { 'constant': false, 'inputs': [ { 'name': 'accountId', 'type': 'address' } ], 'name': 'getMaxConcurrencyCount', 'outputs': [ { 'name': '', 'type': 'uint256' } ], 'payable': false, 'stateMutability': 'nonpayable', 'type': 'function' }, { 'anonymous': false, 'inputs': [ { 'indexed': false, 'name': 'currentCount', 'type': 'uint256' }, { 'indexed': true, 'name': 'user', 'type': 'address' }, { 'indexed': false, 'name': 'action', 'type': 'string' }, { 'indexed': false, 'name': 'result', 'type': 'bool' }, { 'indexed': false, 'name': 'currentUser', 'type': 'string' } ], 'name': 'Transaction', 'type': 'event' } ]";

            var resultUnlocking = await _web3.Personal.UnlockAccount.SendRequestAsync(_accountAddress, _password, 120);

            if (resultUnlocking)
            {
                //return _web3.Eth.GetContract(abi, "0x2349cd9F16e3C4A0dAc13FFf8c241Ed18c9DBe8B");
                //return _web3.Eth.GetContract(abi, "0xB9162DCD3B4Dd64B34Dfc48276e314469c09D022");
                return _web3.Eth.GetContract(abi, "0xa1E83D7507dC5EEbDdC8494ED87272eac692091B");
            }
            return null;
        }

        public async Task<bool> UnlockAccount(string accountId, string password = "123abc123A")
        {
            var resultUnlocking = await _web3.Personal.UnlockAccount.SendRequestAsync(accountId, _password, 200);

            return resultUnlocking;
        }

        public async Task<Nethereum.RPC.Eth.DTOs.TransactionReceipt> MineAndGetReceiptAsync(string transactionHash)
        {
            var web3Geth = new Web3Geth(_web3.Client);

            var miningResult = await web3Geth.Miner.Start.SendRequestAsync(6);

            var receipt = await _web3.Eth.Transactions.GetTransactionReceipt.SendRequestAsync(transactionHash);

            while (receipt == null)
            {
                //Thread.Sleep(5000);
                receipt = await _web3.Eth.Transactions.GetTransactionReceipt.SendRequestAsync(transactionHash);
            }

            miningResult = await web3Geth.Miner.Stop.SendRequestAsync();
            return receipt;
        }

    }

    public class TransactionEvent
    {
        [Parameter("uint256", "currentCount", 1, false)]
        public int CurrentCount { get; set; }

        [Parameter("address", "user", 2, true)]
        public string User { get; set; }

        [Parameter("string", "action", 3, false)]
        public string Action { get; set; }

        [Parameter("bool", "result", 4, false)]
        public bool Result { get; set; }

        [Parameter("string", "currentUser", 5, false)]
        public string CurrentUser { get; set; }

    }


}
