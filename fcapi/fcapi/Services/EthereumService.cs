using Nethereum.Web3;
using System;
using System.Threading.Tasks;
using Nethereum.Contracts;
using Nethereum.Geth;
using System.Numerics;
using System.Threading;
using Nethereum.ABI.FunctionEncoding.Attributes;
using System.Configuration;

namespace EthereumStart.Services
{
    [FunctionOutput]
    public class UserInfo
    {
        [Parameter("string", "name", 1)]
        public string name { get; set; }

        [Parameter("string", "emailId", 2)]
        public string emailId { get; set; }

        [Parameter("string", "phNumber", 3)]
        public string phNumber { get; set; }

        [Parameter("string", "password", 4)]
        public string password { get; set; }

        [Parameter("address", "accountId", 5)]
        public string accountId { get; set; }

        [Parameter("uint", "index", 6)]
        public int index { get; set; }
    }

    public enum ParticipantTypes
    {
        Investor = 0,
        FundManager = 1,
        Beneficiary = 2
    }

    public class BasicEthereumService : IEthereumService
    {
        private Nethereum.Web3.Web3 _web3;
        private string _accountAddress;
        private string _fundmanager;
        private string _password;
        private string _storageKey;
        private string _storageAccount;
        
        public string Investor
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

        public string MiningAddress
        {
            get
            {
                return ConfigurationManager.AppSettings["miningIpAddress"];
            }
        }

        public string AbiContarctAddress
        {
            get
            {
                return ConfigurationManager.AppSettings["abiContractAddress"];
            }
        }

        public string AdminAddress
        {
            get
            {
                return ConfigurationManager.AppSettings["adminAddress"];
            }
        }

        public string FmAddress
        {
            get
            {
                return ConfigurationManager.AppSettings["fmAddress"];
            }
        }

        public string AdminPwd
        {
            get
            {
                return ConfigurationManager.AppSettings["adminPwd"];
            }
        }


        public string Fundmanager
        {
            get
            {
                return _fundmanager;
            }

            set
            {
                _fundmanager = value;
            }
        }

        public BasicEthereumService()
        {
            _web3 = new Web3(MiningAddress);
            //_web3 = new Web3("http://40.121.92.98:8000");
            _accountAddress = AdminAddress;
            _fundmanager = FmAddress;
            _password = AdminPwd;
            //_storageAccount = config.Value.StorageAccount;
            //_storageKey = config.Value.StorageKey;            
            _web3.TransactionManager.DefaultGas = BigInteger.Parse("290000");
            _web3.TransactionManager.DefaultGasPrice = BigInteger.Parse("1");
        }

        //public async Task<bool> SaveContractToTableStorage(EthereumContractInfo contract)
        //{
        //    StorageCredentials credentials = new StorageCredentials(_storageAccount, _storageKey);
        //    CloudStorageAccount account = new CloudStorageAccount(credentials, true);
        //    var client = account.CreateCloudTableClient();

        //    var tableRef = client.GetTableReference("ethtransactions");
        //    await tableRef.CreateIfNotExistsAsync();

        //    TableOperation ops = TableOperation.InsertOrMerge(contract);
        //    await tableRef.ExecuteAsync(ops);
        //    return true;
        //}

        //public async Task<EthereumContractInfo> GetContractFromTableStorage(string name)
        //{
        //    StorageCredentials credentials = new StorageCredentials(_storageAccount, _storageKey);
        //    CloudStorageAccount account = new CloudStorageAccount(credentials, true);
        //    var client = account.CreateCloudTableClient();

        //    var tableRef = client.GetTableReference("ethtransactions");
        //    await tableRef.CreateIfNotExistsAsync();

        //    TableOperation ops = TableOperation.Retrieve<EthereumContractInfo>("contract", name);
        //    var tableResult = await tableRef.ExecuteAsync(ops);
        //    if (tableResult.HttpStatusCode == 200)
        //        return (EthereumContractInfo)tableResult.Result;
        //    else
        //        return null;
        //}

        public async Task<decimal> GetBalance(string address)
        {
            var balance = await _web3.Eth.GetBalance.SendRequestAsync(address);
            return Nethereum.Util.UnitConversion.Convert.FromWei(balance.Value, 18);
        }

        public async Task<string> CreateAccount()
        {
            var accountAddress = await _web3.Personal.NewAccount.SendRequestAsync("12345");
            return accountAddress;
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

        //public async Task<bool> ReleaseContract(string name, string abi, string byteCode, int gas)
        //{

        //    // check contractName
        //    var existing = await this.GetContractFromTableStorage(name);
        //    if (existing != null) throw new Exception($"Contract {name} is present in storage");
        //    try
        //    {
        //        var resultUnlocking = await _web3.Personal.UnlockAccount.SendRequestAsync(_accountAddress, _password, 60);
        //        if (resultUnlocking)
        //        {
        //            var transactionHash = await _web3.Eth.DeployContract.SendRequestAsync(abi, byteCode, _accountAddress, new Nethereum.Hex.HexTypes.HexBigInteger(gas));

        //            EthereumContractInfo eci = new EthereumContractInfo(name, abi, byteCode, transactionHash);
        //            return await SaveContractToTableStorage(eci);
        //        }
        //    }
        //    catch (Exception exc)
        //    {
        //        return false;
        //    }
        //    return false;
        //}

        //public async Task<string> TryGetContractAddress(string name)
        //{
        //    // check contractName
        //    var existing = await this.GetContractFromTableStorage(name);
        //    if (existing == null) throw new Exception($"Contract {name} does not exist in storage");

        //    if (!String.IsNullOrEmpty(existing.ContractAddress))
        //        return existing.ContractAddress;
        //    else
        //    {
        //        var resultUnlocking = await _web3.Personal.UnlockAccount.SendRequestAsync(_accountAddress, _password, 60);
        //        if (resultUnlocking)
        //        {
        //            var receipt = await _web3.Eth.Transactions.GetTransactionReceipt.SendRequestAsync(existing.TransactionHash);
        //            if (receipt != null)
        //            {
        //                existing.ContractAddress = receipt.ContractAddress;
        //                await SaveContractToTableStorage(existing);
        //                return existing.ContractAddress;
        //            }
        //        }
        //    }
        //    return null;
        //}

        public async Task<Contract> GetContract(string name)
        {
            //string abi = @"[ { 'constant': false, 'inputs': [ { 'name': '_cfgName', 'type': 'string' }, { 'name': '_amount', 'type': 'uint256' } ], 'name': 'disburseFund', 'outputs': [ { 'name': '', 'type': 'bool' } ], 'payable': true, 'stateMutability': 'payable', 'type': 'function' }, { 'constant': true, 'inputs': [ { 'name': '_cfgName', 'type': 'string' } ], 'name': 'getCFGBeneficiariesCount', 'outputs': [ { 'name': '', 'type': 'uint256', 'value': '0' } ], 'payable': false, 'stateMutability': 'view', 'type': 'function' }, { 'constant': false, 'inputs': [ { 'name': '_contractAddr', 'type': 'address' }, { 'name': '_cFgName', 'type': 'string' } ], 'name': 'addContractToCFG', 'outputs': [ { 'name': '', 'type': 'bool' } ], 'payable': false, 'stateMutability': 'nonpayable', 'type': 'function' }, { 'constant': false, 'inputs': [ { 'name': '_acctId', 'type': 'address' }, { 'name': '_name', 'type': 'string' }, { 'name': '_emailId', 'type': 'string' }, { 'name': '_phNumber', 'type': 'string' }, { 'name': '_password', 'type': 'string' } ], 'name': 'addFM', 'outputs': [ { 'name': '', 'type': 'uint256' } ], 'payable': false, 'stateMutability': 'nonpayable', 'type': 'function' }, { 'constant': false, 'inputs': [ { 'name': '_fmAddr', 'type': 'address' }, { 'name': '_cFgName', 'type': 'string' } ], 'name': 'CreatePledge', 'outputs': [ { 'name': '', 'type': 'address' } ], 'payable': true, 'stateMutability': 'payable', 'type': 'function' }, { 'constant': false, 'inputs': [ { 'name': '_acctId', 'type': 'address' }, { 'name': '_fmId', 'type': 'address' }, { 'name': '_name', 'type': 'string' }, { 'name': '_emailId', 'type': 'string' }, { 'name': '_phNumber', 'type': 'string' }, { 'name': '_password', 'type': 'string' } ], 'name': 'addBeneficiary', 'outputs': [ { 'name': '', 'type': 'uint256' } ], 'payable': false, 'stateMutability': 'nonpayable', 'type': 'function' }, { 'constant': true, 'inputs': [ { 'name': '_cfgName', 'type': 'string' } ], 'name': 'getCFGBeneficiaries', 'outputs': [ { 'name': '', 'type': 'address[]', 'value': [] } ], 'payable': false, 'stateMutability': 'view', 'type': 'function' }, { 'constant': false, 'inputs': [ { 'name': '_name', 'type': 'string' }, { 'name': '_fmId', 'type': 'address' }, { 'name': '_startDate', 'type': 'uint256' }, { 'name': '_expiryDate', 'type': 'uint256' } ], 'name': 'addCampaignFG', 'outputs': [ { 'name': '', 'type': 'uint256' } ], 'payable': false, 'stateMutability': 'nonpayable', 'type': 'function' }, { 'constant': false, 'inputs': [ { 'name': '_acctId', 'type': 'address' }, { 'name': '_name', 'type': 'string' }, { 'name': '_emailId', 'type': 'string' }, { 'name': '_phNumber', 'type': 'string' }, { 'name': '_password', 'type': 'string' } ], 'name': 'addInvestor', 'outputs': [ { 'name': '', 'type': 'uint256' } ], 'payable': false, 'stateMutability': 'nonpayable', 'type': 'function' }, { 'constant': false, 'inputs': [ { 'name': '_beneAddr', 'type': 'address' }, { 'name': '_cFgName', 'type': 'string' } ], 'name': 'addBeneficiaryToCFG', 'outputs': [ { 'name': '', 'type': 'bool' } ], 'payable': false, 'stateMutability': 'nonpayable', 'type': 'function' }, { 'constant': true, 'inputs': [ { 'name': '_cfgName', 'type': 'string' } ], 'name': 'getCFGContracts', 'outputs': [ { 'name': '', 'type': 'address[]', 'value': [] } ], 'payable': false, 'stateMutability': 'view', 'type': 'function' }, { 'constant': true, 'inputs': [ { 'name': '_id', 'type': 'address' }, { 'name': '_pType', 'type': 'uint8' } ], 'name': 'getUser', 'outputs': [ { 'name': '', 'type': 'string' }, { 'name': '', 'type': 'string' }, { 'name': '', 'type': 'string' }, { 'name': '', 'type': 'string' }, { 'name': '', 'type': 'address' }, { 'name': '', 'type': 'uint256' } ], 'payable': false, 'stateMutability': 'view', 'type': 'function' }, { 'inputs': [], 'payable': true, 'stateMutability': 'payable', 'type': 'constructor' } ] ";
            string abi = @"[ { 'constant': false, 'inputs': [ { 'name': '_acctId', 'type': 'address' }, { 'name': '_name', 'type': 'string' } ], 'name': 'addInvestor', 'outputs': [ { 'name': '', 'type': 'uint256' } ], 'payable': false, 'stateMutability': 'nonpayable', 'type': 'function', 'signature': '0x3130ca2d' }, { 'constant': false, 'inputs': [ { 'name': '_acctId', 'type': 'address' }, { 'name': '_name', 'type': 'string' } ], 'name': 'addBeneficiary', 'outputs': [ { 'name': '', 'type': 'uint256' } ], 'payable': false, 'stateMutability': 'nonpayable', 'type': 'function', 'signature': '0x32b578e5' }, { 'constant': false, 'inputs': [ { 'name': '_cFgName', 'type': 'string' } ], 'name': 'CreatePledge', 'outputs': [ { 'name': '', 'type': 'uint256' } ], 'payable': true, 'stateMutability': 'payable', 'type': 'function', 'signature': '0x37c379c7' }, { 'constant': false, 'inputs': [ { 'name': '_name', 'type': 'string' } ], 'name': 'addCampaignFG', 'outputs': [ { 'name': '', 'type': 'uint256' } ], 'payable': false, 'stateMutability': 'nonpayable', 'type': 'function', 'signature': '0x9090b34e' }, { 'constant': false, 'inputs': [ { 'name': '_beneAddr', 'type': 'address' }, { 'name': '_cFgName', 'type': 'string' } ], 'name': 'addBeneficiaryToCFG', 'outputs': [ { 'name': '', 'type': 'bool' } ], 'payable': false, 'stateMutability': 'nonpayable', 'type': 'function', 'signature': '0xbf1dfc5f' }, { 'constant': false, 'inputs': [ { 'name': '_acctId', 'type': 'address' }, { 'name': '_name', 'type': 'string' } ], 'name': 'addFM', 'outputs': [ { 'name': '', 'type': 'uint256' } ], 'payable': false, 'stateMutability': 'nonpayable', 'type': 'function', 'signature': '0xedf37e86' }, { 'constant': false, 'inputs': [ { 'name': '_cfgName', 'type': 'string' } ], 'name': 'disburseFund', 'outputs': [ { 'name': '', 'type': 'uint256' } ], 'payable': true, 'stateMutability': 'payable', 'type': 'function', 'signature': '0xf80d3662' } ]";
            var resultUnlocking = await _web3.Personal.UnlockAccount.SendRequestAsync(_accountAddress, _password, 3600);
            var resultUnlocking1 = await _web3.Personal.UnlockAccount.SendRequestAsync(_fundmanager, _password, 3600);

            if (resultUnlocking)
            {
                //return _web3.Eth.GetContract(abi, '0x01eAef116F163bebc875188e1Fc08b12f0AAf291");
                return _web3.Eth.GetContract(abi, AbiContarctAddress);
            }
            return null;
        }

        public async Task<bool> UnlockAccount(string accountId, string password = "12345")
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
