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
        [Route("getSellerCount/{tfname}/{name}")]
        public async Task<decimal> getSellerCount(string tfname, string name)
        {
            return await service.GetBalance(service.Investor);
        }

        [HttpGet]
        [Route("getSellerBalance/{tfname}/{name}")]
        public async Task<decimal> getSellerBalance(string tfname, string name)
        {
            return await service.GetBalance(service.Fundmanager);
        }

        [HttpGet]
        [Route("getBuyerBalance/{tfname}/{name}")]
        public async Task<decimal> getBuyerBalance(string tfname, string name)
        {
            return await service.GetBalance(service.Fundmanager);
        }

        [HttpGet]
        [Route("getBuyerCount/{tfname}/{name}")]
        public async Task<decimal> getBuyerCount(string tfname, string name)
        {
            return await service.GetBalance(service.Investor);
        }

        

        /// <summary>
        /// use this method for setConcurrency
        /// </summary>
        /// <param name="count"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("addTradingFloor/name/{name}")]
        public async Task<bool> addTradingFloor(string name = "")
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
        [Route("addSellerTF/addr/{addr}/tfname/{tfname}/name/{name}")]
        public async Task<bool> addSellerTF(string addr = "", string tfname = "", string name = "")
        {
            try
            {
                var contract = await service.GetContract("");
                if (contract == null) throw new System.Exception("Contract not present in storage");
                var method = contract.GetFunction("addBeneficiaryToCFG");

                var transactionHash = await method.SendTransactionAsync(service.Fundmanager, addr, tfname);
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
        [Route("addBuyerTF/addr/{addr}/tfname/{tfname}/name/{name}")]
        public async Task<bool> addBuyerTF(string addr = "", string tfname = "", string name = "")
        {
            try
            {
                var contract = await service.GetContract("");
                if (contract == null) throw new System.Exception("Contract not present in storage");
                var method = contract.GetFunction("addBeneficiaryToCFG");

                var transactionHash = await method.SendTransactionAsync(service.Fundmanager, addr, tfname);
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
