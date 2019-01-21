using EthereumStart.Services;
using Nethereum.Hex.HexTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Numerics;
using System.Threading.Tasks;
using System.Web.Http;

namespace fcapi.Controllers
{
    public class TradingController : ApiController
    {
        private IEthereumService service;

        public TradingController()
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
        [Route("getSellerCount/{tfname}")]
        public async Task<string> getSellerCount(string tfname)
        {
            try
            {
                var contract = await service.GetContract();
                if (contract == null) throw new System.Exception("Contract not present in storage");
                var method = contract.GetFunction("getSellerCount");

                var sellerCount = await method.CallAsync<int>(tfname);
                return sellerCount.ToString();
            }
            catch (Exception ex)
            {
                return "Error when getting seller count " + ex.Message;
            }

        }

        [HttpGet]
        [Route("getSellerBalance/{tfname}/{name}")]
        public async Task<string> getSellerBalance(string tfname, string name)
        {
            try
            { 
                var contract = await service.GetContract();
                if (contract == null) throw new System.Exception("Contract not present in storage");
                var method = contract.GetFunction("getSellerBalance");

                var sellerBalance = await method.CallAsync<BigInteger>(tfname, name);
                var sBalance = Nethereum.Util.UnitConversion.Convert.FromWei(sellerBalance, 18);
                return sBalance.ToString();
            }
            catch (Exception ex)
            {
                return "Error when getting seller balance " + ex.Message;
            }
        }

        [HttpGet]
        [Route("getBuyerBalance/{tfname}/{name}")]
        public async Task<string> getBuyerBalance(string tfname, string name)
        {
            try
            { 
                var contract = await service.GetContract();
                if (contract == null) throw new System.Exception("Contract not present in storage");
                var method = contract.GetFunction("getBuyerBalance");

                //var sellerBalance = await method.CallAsync<int>(tfname, name);
                //return sellerBalance.ToString();

                var buyerBalance = await method.CallAsync<BigInteger>(tfname, name);
                var bBalance = Nethereum.Util.UnitConversion.Convert.FromWei(buyerBalance, 18);
                return bBalance.ToString();
            }
            catch (Exception ex)
            {
                return "Error when getting buyer balace " + ex.Message;
            }
        }

        [HttpGet]
        [Route("getBuyerCount/{tfname}")]
        public async Task<string> getBuyerCount(string tfname)
        {
            try
            { 
                var contract = await service.GetContract();
                if (contract == null) throw new System.Exception("Contract not present in storage");
                var method = contract.GetFunction("getBuyerCount");

                var sellerCount = await method.CallAsync<int>(tfname);
                return sellerCount.ToString();
            }
            catch (Exception ex)
            {
                return "Error when getting buyer count " + ex.Message;
            }
        }        

        /// <summary>
        /// use this method for setConcurrency
        /// </summary>
        /// <param name="count"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("addTradingFloor/{name}")]
        public async Task<string> addTradingFloor(string name = "")
        {
            try
            {
                var contract = await service.GetContract();
                if (contract == null) throw new System.Exception("Contract not present in storage");
                var method = contract.GetFunction("createTradingFloor");

                var transactionHash = await method.SendTransactionAsync(service.Investor, name);
                var receipt = await service.MineAndGetReceiptAsync(transactionHash);
                return "Successfully created Trading Floor";
            }
            catch (Exception ex)
            {
                return "Error when creating Trading Floor " + ex.Message;
            }
        }

        /// <summary>
        /// use this method for setConcurrency
        /// </summary>
        /// <param name="count"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("addSellerToTF/tfname/{tfname}/name/{name}")]
        public async Task<bool> addSellerToTF(string tfname = "", string name = "")
        {
            try
            {
                var contract = await service.GetContract("");
                if (contract == null) throw new System.Exception("Contract not present in storage");
                var method = contract.GetFunction("addSellerToTF");

                var accountAddr = await service.CreateAccount();

                var transactionHash = await method.SendTransactionAsync(service.Investor, tfname, name, accountAddr);
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
        [HttpPost]
        [Route("addBuyerToTF/tfname/{tfname}/name/{name}")]
        public async Task<bool> addBuyerToTF(string tfname = "", string name = "")
        {
            try
            {
                var contract = await service.GetContract("");
                if (contract == null) throw new System.Exception("Contract not present in storage");
                var method = contract.GetFunction("addBuyerToTF");

                var accountAddr = await service.CreateAccount();

                var transactionHash = await method.SendTransactionAsync(service.Investor, tfname, name, accountAddr);
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
        [HttpPost]
        [Route("addFundsToSeller/tfname/{tfname}/name/{name}/amount/{amount}")]
        public async Task<bool> addFundsToSeller(string tfname = "", string name = "", string amount = "")
        {
            try
            {
                var contract = await service.GetContract("");
                if (contract == null) throw new System.Exception("Contract not present in storage");
                var method = contract.GetFunction("addFundsToSeller");

                var weiAmount = Nethereum.Util.UnitConversion.Convert.ToWei(amount, Nethereum.Util.UnitConversion.EthUnit.Ether);
                var transactionHash = await method.SendTransactionAsync(service.Investor, null, new Nethereum.Hex.HexTypes.HexBigInteger(weiAmount), tfname, name);
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
        [HttpPost]
        [Route("addFundsToBuyer/tfname/{tfname}/name/{name}/amount/{amount}")]
        public async Task<bool> addFundsToBuyer(string tfname = "", string name = "", string amount = "")
        {
            try
            {
                var contract = await service.GetContract("");
                if (contract == null) throw new System.Exception("Contract not present in storage");
                var method = contract.GetFunction("addFundsToBuyer");

                var weiAmount = Nethereum.Util.UnitConversion.Convert.ToWei(amount, Nethereum.Util.UnitConversion.EthUnit.Ether);
                var transactionHash = await method.SendTransactionAsync(service.Investor, null, new Nethereum.Hex.HexTypes.HexBigInteger(weiAmount), tfname, name);
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
