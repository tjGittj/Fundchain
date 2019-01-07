using EthereumStart.Services;
using Nethereum.Hex.HexTypes;
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
        [Route("getInvestorBalance")]
        public async Task<decimal> GetInvestorBalance()
        {
            return await service.GetBalance(service.Investor);
        }


        [HttpGet]
        [Route("getFundManagerBalance")]
        public async Task<decimal> GetFundManagerBalance()
        {
            return await service.GetBalance(service.Fundmanager);
        }

        #region commented section
        //[HttpGet]
        //[Route("getUser/{user}/type/{type}")]
        //public async Task<UserInfo> GetUser(string user = "", string type = "0")
        //{
        //    if (string.IsNullOrEmpty(user))
        //    {
        //        user = service.Investor;
        //    }
        //    var contract = await service.GetContract("");
        //    if (contract == null) throw new System.Exception("Contract not present in storage");
        //    var method = contract.GetFunction("getUser");
        //    try
        //    {
        //        var result = method.CallDeserializingToObjectAsync<UserInfo>(user, byte.Parse(type)).GetAwaiter().GetResult();
        //        return result;
        //    }
        //    catch (Exception ex)
        //    {
        //        return null;
        //    }
        //}

        /// <summary>
        /// use this method for setConcurrency
        /// </summary>
        /// <param name="count"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        //[HttpGet]
        //[Route("addFM/name/{name}/emailId/{emailId}/phNumber/{phNumber}/password/{password}")]
        //public async Task<bool> addFM(string name = "", string emailId = "", string phNumber = "", string password = "")
        //{
        //    try
        //    {
        //       var address = await service.CreateAccount();
        //        var contract = await service.GetContract("");
        //        if (contract == null) throw new System.Exception("Contract not present in storage");
        //        var method = contract.GetFunction("addFM");                

        //        var transactionHash = await method.SendTransactionAsync(service.AccountAddress, address, name, emailId, phNumber, password);
        //        var receipt = await service.MineAndGetReceiptAsync(transactionHash);
        //        return true;
        //    }
        //    catch (Exception ex)
        //    {
        //        return false;
        //    }
        //}

        /// <summary>
        /// use this method for setConcurrency
        /// </summary>
        /// <param name="count"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        //[HttpGet]
        //[Route("addInvestor/name/{name}/emailId/{emailId}/phNumber/{phNumber}/password/{password}")]
        //public async Task<bool> addInvestor(string name = "", string emailId = "", string phNumber = "", string password = "")
        //{
        //    try
        //    {
        //        var address = await service.CreateAccount();
        //        var contract = await service.GetContract("");
        //        if (contract == null) throw new System.Exception("Contract not present in storage");
        //        var method = contract.GetFunction("addInvestor");

        //        var transactionHash = await method.SendTransactionAsync(service.AccountAddress, address, name, emailId, phNumber, password);
        //        var receipt = await service.MineAndGetReceiptAsync(transactionHash);
        //        return true;
        //    }
        //    catch (Exception ex)
        //    {
        //        return false;
        //    }
        //}

        /// <summary>
        /// use this method for setConcurrency
        /// </summary>
        /// <param name="count"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        //[HttpGet]
        //[Route("addBeneficiary/fm/{fm}/name/{name}/emailId/{emailId}/phNumber/{phNumber}/password/{password}")]
        //public async Task<bool> addBeneficiary(string fm = "", string name = "", string emailId = "", string phNumber = "", string password = "")
        //{
        //    try
        //    {
        //        var address = await service.CreateAccount();
        //        var contract = await service.GetContract("");
        //        if (contract == null) throw new System.Exception("Contract not present in storage");
        //        var method = contract.GetFunction("addBeneficiary");

        //        var transactionHash = await method.SendTransactionAsync(service.Fundmanager, address, fm, name, emailId, phNumber, password);
        //        var receipt = await service.MineAndGetReceiptAsync(transactionHash);
        //        return true;
        //    }
        //    catch (Exception ex)
        //    {
        //        return false;
        //    }
        //}
        #endregion commented section

        /// <summary>
        /// use this method for setConcurrency
        /// </summary>
        /// <param name="count"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("addCampaignFG/name/{name}")]
        public async Task<bool> addCampaignFG(string name = "")
        {
            try
            {
                var contract = await service.GetContract("");
                if (contract == null) throw new System.Exception("Contract not present in storage");
                var method = contract.GetFunction("addCampaignFG");

                var transactionHash = await method.SendTransactionAsync(service.Fundmanager, name);
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
        [Route("createPledge/cfgname/{cfgname}/amount/{amount}")]
        public async Task<bool> createPledge(string cfgname = "", string amount = "")
        {
            try
            {
                var contract = await service.GetContract("");
                if (contract == null) throw new System.Exception("Contract not present in storage");
                var method = contract.GetFunction("CreatePledge");

                var weiAmount = Nethereum.Util.UnitConversion.Convert.ToWei(amount, Nethereum.Util.UnitConversion.EthUnit.Ether);
                var transactionHash = await method.SendTransactionAsync(service.Investor, null, new Nethereum.Hex.HexTypes.HexBigInteger(weiAmount), cfgname);
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
                var method = contract.GetFunction("addBeneficiaryToCFG");

                var transactionHash = await method.SendTransactionAsync(service.Fundmanager, beneAddr, cfgname);
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
        //[HttpGet]
        //[Route("addContractToCFG/contractAddr/{contractAddr}/cfgname/{cfgname}")]
        //public async Task<bool> addContractToCFG(string contractAddr = "", string cfgname = "")
        //{
        //    try
        //    {
        //        var contract = await service.GetContract("");
        //        if (contract == null) throw new System.Exception("Contract not present in storage");
        //        var method = contract.GetFunction("addContractToCFG");

        //        var transactionHash = await method.SendTransactionAsync(service.AccountAddress, contractAddr, cfgname);
        //        var receipt = await service.MineAndGetReceiptAsync(transactionHash);
        //        return true;
        //    }
        //    catch (Exception ex)
        //    {
        //        return false;
        //    }
        //}

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
                var method = contract.GetFunction("disburseFund");

                var weiAmount = Nethereum.Util.UnitConversion.Convert.ToWei(amount, Nethereum.Util.UnitConversion.EthUnit.Ether);
                var transactionHash = await method.SendTransactionAsync(service.Fundmanager, null, new Nethereum.Hex.HexTypes.HexBigInteger(weiAmount), cfgname);
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
