using EthereumStart.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace fcapi.Controllers
{
    public class FundchainController : ApiController
    {
        private IEthereumService service;

        public FundchainController()
        {
            service = new BasicEthereumService();
        }

        [HttpGet]
        [Route("getBalance/{walletAddress}")]
        public async Task<decimal> GetBalance(string walletAddress)
        {
            return await service.GetBalance(walletAddress);
        }

        [HttpGet]
        [Route("getUser/{user}/type/{type}")]
        public async Task<UserInfo> GetUser(string user = "", string type = "0")
        {
            if (string.IsNullOrEmpty(user))
            {
                user = service.AccountAddress;
            }
            var contract = await service.GetContract("");
            if (contract == null) throw new System.Exception("Contract not present in storage");
            var method = contract.GetFunction("getUser");
            try
            {
                var result = method.CallDeserializingToObjectAsync<UserInfo>(user, byte.Parse(type)).GetAwaiter().GetResult();
                return result;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        /// <summary>
        /// use this method for setConcurrency
        /// </summary>
        /// <param name="count"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("addFM/address/{address}/name/{name}/emailId/{emailId}/phNumber/{phNumber}/password/{password}")]
        public async Task<bool> addFM(string address = "", string name = "", string emailId = "", string phNumber = "", string password = "")
        {
            try
            {
                var contract = await service.GetContract("");
                if (contract == null) throw new System.Exception("Contract not present in storage");
                var method = contract.GetFunction("addFM");                

                var transactionHash = await method.SendTransactionAsync(service.AccountAddress, address, name, emailId, phNumber, password);
                var receipt = await service.MineAndGetReceiptAsync(transactionHash);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        /// <summary>
        /// use this method for setConcurrency
        /// </summary>
        /// <param name="count"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("addInvestor/address/{address}/name/{name}/emailId/{emailId}/phNumber/{phNumber}/password/{password}")]
        public async Task<bool> addInvestor(string address = "", string name = "", string emailId = "", string phNumber = "", string password = "")
        {
            try
            {
                var contract = await service.GetContract("");
                if (contract == null) throw new System.Exception("Contract not present in storage");
                var method = contract.GetFunction("addInvestor");

                var transactionHash = await method.SendTransactionAsync(service.AccountAddress, address, name, emailId, phNumber, password);
                var receipt = await service.MineAndGetReceiptAsync(transactionHash);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        /// <summary>
        /// use this method for setConcurrency
        /// </summary>
        /// <param name="count"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("addBeneficiary/address/{address}/fm/{fm}/name/{name}/emailId/{emailId}/phNumber/{phNumber}/password/{password}")]
        public async Task<bool> addBeneficiary(string address = "", string fm = "", string name = "", string emailId = "", string phNumber = "", string password = "")
        {
            try
            {
                var contract = await service.GetContract("");
                if (contract == null) throw new System.Exception("Contract not present in storage");
                var method = contract.GetFunction("addInvestor");

                var transactionHash = await method.SendTransactionAsync(service.AccountAddress, address, fm, name, emailId, phNumber, password);
                var receipt = await service.MineAndGetReceiptAsync(transactionHash);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        /// <summary>
        /// use this method for setConcurrency
        /// </summary>
        /// <param name="count"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("addCampaignFG/name/{name}/fm/{fm}/startDate/{startDate}/expiryDate/{expiryDate}")]
        public async Task<bool> addCampaignFG(string name = "", string fm = "", string startDate = "", string expiryDate = "")
        {
            try
            {
                var contract = await service.GetContract("");
                if (contract == null) throw new System.Exception("Contract not present in storage");
                var method = contract.GetFunction("addInvestor");

                var transactionHash = await method.SendTransactionAsync(service.AccountAddress, name, fm, startDate, expiryDate);
                var receipt = await service.MineAndGetReceiptAsync(transactionHash);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        /// <summary>
        /// use this method for setConcurrency
        /// </summary>
        /// <param name="count"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("createPledge/fm/{fm}/cfgname/{cfgname}")]
        public async Task<bool> createPledge(string fm = "", string cfgname = "")
        {
            try
            {
                var contract = await service.GetContract("");
                if (contract == null) throw new System.Exception("Contract not present in storage");
                var method = contract.GetFunction("CreatePledge");

                var transactionHash = await method.SendTransactionAsync(service.AccountAddress, fm, cfgname);
                var receipt = await service.MineAndGetReceiptAsync(transactionHash);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        /// <summary>
        /// use this method for setConcurrency
        /// </summary>
        /// <param name="count"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("addBeneficiaryToCFG/beneAddr/{beneAddr}/cfgname/{cfgname}")]
        public async Task<bool> addBeneficiaryToCFG(string beneAddr = "", string cfgname = "")
        {
            try
            {
                var contract = await service.GetContract("");
                if (contract == null) throw new System.Exception("Contract not present in storage");
                var method = contract.GetFunction("CreatePledge");

                var transactionHash = await method.SendTransactionAsync(service.AccountAddress, beneAddr, cfgname);
                var receipt = await service.MineAndGetReceiptAsync(transactionHash);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        /// <summary>
        /// use this method for setConcurrency
        /// </summary>
        /// <param name="count"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("addContractToCFG/contractAddr/{contractAddr}/cfgname/{cfgname}")]
        public async Task<bool> addContractToCFG(string contractAddr = "", string cfgname = "")
        {
            try
            {
                var contract = await service.GetContract("");
                if (contract == null) throw new System.Exception("Contract not present in storage");
                var method = contract.GetFunction("CreatePledge");

                var transactionHash = await method.SendTransactionAsync(service.AccountAddress, contractAddr, cfgname);
                var receipt = await service.MineAndGetReceiptAsync(transactionHash);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        /// <summary>
        /// use this method for setConcurrency
        /// </summary>
        /// <param name="count"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("disburseFund/cfgname/{cfgname}/amount/{amount}")]
        public async Task<bool> disburseFund(string cfgname = "", string amount = "")
        {
            try
            {
                var contract = await service.GetContract("");
                if (contract == null) throw new System.Exception("Contract not present in storage");
                var method = contract.GetFunction("CreatePledge");

                var transactionHash = await method.SendTransactionAsync(service.AccountAddress, cfgname, amount);
                var receipt = await service.MineAndGetReceiptAsync(transactionHash);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}
