using EthereumStart.Models;
using EthereumStart.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Nethereum.Geth;
using Nethereum.Hex.HexTypes;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.Web3;
using System;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;


namespace EthereumStart.Controllers
{
    [Route("api/[controller]")]
    public class EthereumTestController : Controller
    {
        private IEthereumService service;
        private const string abi = @"[ { 'constant': false, 'inputs': [], 'name': 'logout', 'outputs': [ { 'name': '', 'type': 'bool' } ], 'payable': false, 'stateMutability': 'nonpayable', 'type': 'function' }, { 'constant': true, 'inputs': [ { 'name': '', 'type': 'address' } ], 'name': 'currentCount', 'outputs': [ { 'name': '', 'type': 'uint256', 'value': '0' } ], 'payable': false, 'stateMutability': 'view', 'type': 'function' }, { 'constant': true, 'inputs': [ { 'name': '', 'type': 'address' } ], 'name': 'maxCount', 'outputs': [ { 'name': '', 'type': 'uint256', 'value': '0' } ], 'payable': false, 'stateMutability': 'view', 'type': 'function' }, { 'constant': false, 'inputs': [ { 'name': 'accountId', 'type': 'address' }, { 'name': 'count', 'type': 'uint256' } ], 'name': 'setConcurrency', 'outputs': [], 'payable': false, 'stateMutability': 'nonpayable', 'type': 'function' }, { 'constant': true, 'inputs': [], 'name': 'getConcurrencyBalance', 'outputs': [ { 'name': '', 'type': 'uint256', 'value': '0' } ], 'payable': false, 'stateMutability': 'view', 'type': 'function' }, { 'constant': false, 'inputs': [], 'name': 'login', 'outputs': [ { 'name': '', 'type': 'bool' } ], 'payable': false, 'stateMutability': 'nonpayable', 'type': 'function' }, { 'anonymous': false, 'inputs': [ { 'indexed': false, 'name': 'currentCount', 'type': 'uint256' }, { 'indexed': true, 'name': 'user', 'type': 'address' } ], 'name': 'Transaction', 'type': 'event' } ]";
        private const string byteCode = "0x60606040526040805190810160405280600481526020017f74657374000000000000000000000000000000000000000000000000000000008152506000908051906020019061004f929190610060565b50341561005b57600080fd5b610105565b828054600181600116156101000203166002900490600052602060002090601f016020900481019282601f106100a157805160ff19168380011785556100cf565b828001600101855582156100cf579182015b828111156100ce5782518255916020019190600101906100b3565b5b5090506100dc91906100e0565b5090565b61010291905b808211156100fe5760008160009055506001016100e6565b5090565b90565b6103f8806101146000396000f30060606040526004361061004c576000357c0100000000000000000000000000000000000000000000000000000000900463ffffffff168063cd048de614610051578063ed40a8c814610127575b600080fd5b341561005c57600080fd5b6100ac600480803590602001908201803590602001908080601f016020809104026020016040519081016040528093929190818152602001838380828437820191505050505050919050506101aa565b6040518080602001828103825283818151815260200191508051906020019080838360005b838110156100ec5780820151818401526020810190506100d1565b50505050905090810190601f1680156101195780820380516001836020036101000a031916815260200191505b509250505060405180910390f35b61012f61026b565b6040518080602001828103825283818151815260200191508051906020019080838360005b8381101561016f578082015181840152602081019050610154565b50505050905090810190601f16801561019c5780820380516001836020036101000a031916815260200191505b509250505060405180910390f35b6101b2610313565b81600090805190602001906101c8929190610327565b5060008054600181600116156101000203166002900480601f01602080910402602001604051908101604052809291908181526020018280546001816001161561010002031660029004801561025f5780601f106102345761010080835404028352916020019161025f565b820191906000526020600020905b81548152906001019060200180831161024257829003601f168201915b50505050509050919050565b610273610313565b60008054600181600116156101000203166002900480601f0160208091040260200160405190810160405280929190818152602001828054600181600116156101000203166002900480156103095780601f106102de57610100808354040283529160200191610309565b820191906000526020600020905b8154815290600101906020018083116102ec57829003601f168201915b5050505050905090565b602060405190810160405280600081525090565b828054600181600116156101000203166002900490600052602060002090601f016020900481019282601f1061036857805160ff1916838001178555610396565b82800160010185558215610396579182015b8281111561039557825182559160200191906001019061037a565b5b5090506103a391906103a7565b5090565b6103c991905b808211156103c55760008160009055506001016103ad565b5090565b905600a165627a7a72305820df710e4b589c2e28b1660d4a1aeac527fe565012fb60208f65c30bac81d8ac920029";
        private const int gas = 4700000;

        public EthereumTestController(IEthereumService ethereumService)
        {
            service = ethereumService;
        }

        [HttpGet]
        [Route("getBalance/{walletAddress}")]
        public async Task<decimal> GetBalance([FromRoute]string walletAddress)
        {
            return await service.GetBalance(walletAddress);
        }

        /// <summary>
        /// use this method for setConcurrency
        /// </summary>
        /// <param name="count"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("exeContract/setConcurrency/{count}/{user}/{currentUser}")]
        public async Task<bool> ExecuteContractCount([FromRoute] int count, [FromRoute] string user = "", string currentUser = "")
        {
            if (string.IsNullOrEmpty(user))
            {
                user = service.AccountAddress;
            }
            try
            {
                var contract = await service.GetContract("");
                if (contract == null) throw new System.Exception("Contract not present in storage");
                var method = contract.GetFunction("setConcurrency");
                var transactionEvent = contract.GetEvent("Transaction");
                var filter = await transactionEvent.CreateFilterAsync();

                var transactionHash = await method.SendTransactionAsync(service.AccountAddress, user, count, currentUser);
                var receipt = await service.MineAndGetReceiptAsync(transactionHash);
                var log = await transactionEvent.GetFilterChanges<TransactionEvent>(filter);

                if (log.Count > 0)
                {
                    return log[0].Event.Result;
                }

                return false;
            }
            catch (Exception ex)
            {
                return false;
            }
        }


        /// <summary>
        /// use this method for incremnet the concurrency user count
        /// </summary>
        /// <param name="count"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("exeContract/incrementConcurrency/{count}/{user}/{currentUser}")]
        public async Task<bool> IncrementConCurrency([FromRoute] int count, [FromRoute] string user = "", string currentUser = "")
        {
            if (string.IsNullOrEmpty(user))
            {
                user = service.AccountAddress;
            }
            try
            {
                var contract = await service.GetContract("");
                if (contract == null) throw new System.Exception("Contract not present in storage");
                var method = contract.GetFunction("incrementConcurrency");
                var transactionEvent = contract.GetEvent("Transaction");
                var filter = await transactionEvent.CreateFilterAsync();

                var transactionHash = await method.SendTransactionAsync(service.AccountAddress, user, count, currentUser);
                var receipt = await service.MineAndGetReceiptAsync(transactionHash);
                var log = await transactionEvent.GetFilterChanges<TransactionEvent>(filter);

                if (log.Count > 0)
                {
                    return log[0].Event.Result;
                }

                return false;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        /// <summary>
        /// use this method for login and logout
        /// </summary>
        /// <param name="contractMethod"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("exeContract/{contractMethod}/{user}/{currentUser}")]
        public async Task<bool> ExecuteContractAddress([FromRoute] string contractMethod, string user = "", string currentUser = "")
        {
            if (string.IsNullOrEmpty(user))
            {
                user = service.AccountAddress;
            }
            try
            {
                var contract = await service.GetContract("");
                if (contract == null) throw new System.Exception("Contract not present in storage");
                var method = contract.GetFunction(contractMethod);
                var transactionEvent = contract.GetEvent("Transaction");
                var filter = await transactionEvent.CreateFilterAsync();
                var transactionHash = await method.SendTransactionAsync(service.AccountAddress, user, currentUser);
                var receipt = await service.MineAndGetReceiptAsync(transactionHash);
                var log = await transactionEvent.GetFilterChanges<TransactionEvent>(filter);

                if (log.Count > 0)
                {
                    return log[0].Event.Result;
                }

                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }


        [HttpGet]
        [Route("exeContract/GetConcurrentUserBalance/{user}")]
        public async Task<int> GetConcurrentUserBalance(string user = "")
        {
            if (string.IsNullOrEmpty(user))
            {
                user = service.AccountAddress;
            }
            var contract = await service.GetContract("");
            if (contract == null) throw new System.Exception("Contract not present in storage");
            var method = contract.GetFunction("getConcurrencyBalance");
            try
            {
                var result = method.CallAsync<int>(user).GetAwaiter().GetResult();
                return result;
            }
            catch (Exception ex)
            {
                return 0;
            }
        }

        [HttpGet]
        [Route("exeContract/GetMaxConcurrentCount/{user}")]
        public async Task<int> GetMaxConcurrentCount(string user = "")
        {
            try
            {
                if (string.IsNullOrEmpty(user))
                {
                    user = service.AccountAddress;
                }
                var contract = await service.GetContract("");
                if (contract == null) throw new System.Exception("Contract not present in storage");
                var method = contract.GetFunction("getMaxConcurrencyCount");

                var result = method.CallAsync<int>(user).GetAwaiter().GetResult();
                return result;
            }
            catch (Exception ex)
            {
                return 0;
            }
        }

        [HttpGet]
        [Route("exeContract/TransferEther/{user}/{accountTo}/{ethers}")]
        public async Task<bool> TransferEther(string user = "", string accountTo = "", int ethers = 0)
        {
            if (string.IsNullOrEmpty(user))
            {
                user = service.AccountAddress;
            }

            try
            {
                var result = await service.TransferEthers(user, accountTo, ethers);
                return result;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}
