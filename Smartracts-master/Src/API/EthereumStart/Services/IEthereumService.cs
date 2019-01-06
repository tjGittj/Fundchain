using System.Threading.Tasks;
using EthereumStart.Models;
using Nethereum.Contracts;
using Nethereum.Web3;

namespace EthereumStart.Services
{
    public interface IEthereumService
    {
        string AccountAddress { get; set; }
        Task<bool> SaveContractToTableStorage(EthereumContractInfo contract);
        Task<EthereumContractInfo> GetContractFromTableStorage(string name);
        Task<decimal> GetBalance(string address);

        Task<bool> TransferEthers(string addressFrom, string addressTo, int ethers);

        Task<bool> ReleaseContract(string name, string abi, string byteCode, int gas);
        Task<string> TryGetContractAddress(string name);
        Task<Contract> GetContract(string name);

        Task<Nethereum.RPC.Eth.DTOs.TransactionReceipt> MineAndGetReceiptAsync(string transactionHash);
    }
}
