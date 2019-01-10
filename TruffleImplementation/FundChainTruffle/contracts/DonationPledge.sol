pragma solidity ^0.4.21;

//Contract for donation 
contract DonationPledge {
    uint public value;
    address public investor;
    address public fundManager;
    string public campaignFG;
    mapping(address => tranInfo) public transactionInfo; 
    address[] tranInfoIndex;
    enum State { Created, Locked, Inactive }
    State public state;
    uint public balance;
    
    struct tranInfo{
        address beneficiaryId;
        uint amountDisbursed;
    }


    constructor() public payable {
        require(value > 0);
        value = msg.value;
        balance = msg.value;
        
    }
     
    //adds details to the instance
    //TRANSFERS Fund to Fund Manager
    //returns the contract address
    function addDetails(address _invAddr, address _fmAddr, string _cFgName)
    public
    returns (address)
    {
        investor = _invAddr;
        fundManager = _fmAddr;
        campaignFG = _cFgName;
        intialTransfer(value);
        return address(this);
    }

    //adds the transactions performed for each beneficiary.
    function addTransactionInfo (address _beneficiary, uint _amount)
    public
    {
        transactionInfo[_beneficiary].beneficiaryId = _beneficiary;
        transactionInfo[_beneficiary].amountDisbursed = _amount; 
        tranInfoIndex.push(_beneficiary);
    }

    //returns the contract information.
    function getContractDetails()
    public
    view
    returns (address, address, string, uint, uint)
    {
        return(investor, fundManager,campaignFG, value, balance);
    }
    
    //gets all the transactions performed against this contract.
    function getContractTransactions()
    public view
    returns(address[], uint[] )
    {
        address[] memory _beneficiaries;
        uint[] memory _tranAmt;
        for(uint i = 0; i<tranInfoIndex.length; i++)
        {
            tranInfo memory info = transactionInfo[tranInfoIndex[i]];
            _beneficiaries[i] = info.beneficiaryId;
            _tranAmt[i] = info.amountDisbursed;
        }
        
        return (_beneficiaries, _tranAmt);
    }
    
    //transfers the amount pledged by investor to the fund manager account
    //while creating the pledging.    
    function intialTransfer(uint _amount)
    private 
    returns(uint, uint)
    {
        fundManager.transfer(_amount);
        return (fundManager.balance, investor.balance);
    }
    
    //updates the current balance.
    function updateBalance(uint _amount)
    public 
    returns(uint)
    {
        balance = _amount;
        return (balance);
    }
    
    //deducts the provided amount from balance.
    function reduceBalance(uint _amount)
    public 
    returns(uint)
    {
        balance = balance - _amount;
        return (balance);
    }
}