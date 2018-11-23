pragma solidity ^0.4.18;

//Contract for donation 
contract Master{
    function Master() public payable {}
    enum ParticipantTypes { Investor, FM, Beneficiary }
    uint priorBalance = 0;
    
    //begin -region  move user management to new contract????
    //represents the registered user.
    struct Participant{
		string name;//required
		string emailId;//required
		string phNumber;//required
		address acctId;//autogen
		string idProof;//required if the investor needs a statement, mandatory for Fm and beneficiary 
		string password;//autogen
		//uint pType;//identifies the Participant as Investor =1/FM =2/Beneficiary=3
		uint index;
	}
	
	struct Investor{
	    Participant investorProfile;
	    address[] transactions;// transactions happened 
	    address[] contracts;//pledged contracts
	}
	
	struct Beneficiary{
	    Participant beneProfile;
	}
	
	struct FM{
	    Participant fmProfile;
	    address[] beneficiaries;
	    mapping(address=>address) transactions;
	    uint balance;
	}
	
	struct CampaignFG{
	    string name;
	    address[] beneficiaries;
	    address[] contracts;
	    address fmId;
	    uint index;
	    uint256 startDate;
	    uint256 expiryDate;
	    bool active;
	    uint balance;
	  
	}
	
	//contracct and balance info that has balance but insufficient.
	struct bufferContract{
	    uint amount;
	}
	
    //Collection of all registered users.
    //this in-memory array is the list of all registered users
    //against which the authentication will be validated.
    mapping(address => Investor) private RegisteredInvestors;
    mapping(address => FM) private RegisteredFMs;
    mapping(address => Beneficiary) private RegisteredBeneficiaries;
    mapping(string => CampaignFG) private RegisteredCampaignFGs;//need another key?
    
    //to handle balances and transactions.
    //this collection is the contracts that have balances but insufficient.
    mapping(address=>bufferContract) bufferContracts;
    address[] private bufferContractsIndex;
    
    //is used to manipulate the length of the collection
    //since mapping does not support iteration.
    address[] private invIndex;
    address[] private fmIndex;
    address[] private beneIndex;
    string[] private cFGIndex;
    
    
     //transfer fund to beneficiaries
    function disburseFund(string cfgName, uint amount)
    public
    payable
    returns(bool success)
    {
        
        address[] memory beneficiaries = getBeneficiaries(cfgName);
        address[] memory contracts = getcfgContracts(cfgName);
        uint bCount = beneficiaries.length;
        uint cCount = contracts.length;
        uint bPosition= 0;//to track if beneficiaries run out
        
        for(uint i = 0; i < cCount; i++)
        {
            // if there are no more beneficiaries to transact.
            if(bPosition >= bCount)
            {
                break;
            }
            
            address contractAddr = contracts[i];
            DonationPledge dp;
            dp = DonationPledge(contractAddr);
            uint  balance = dp.balance();
            if (balance <= 0 )
            {
                continue;
            }
            
            if(priorBalance + balance >= amount)
            {
                    
                if(priorBalance <= 0)
                {
                    while(balance >= amount)
                    {
                        beneficiaries[bPosition].transfer(amount);
                        //updates contract state with beneficiary, amount
                        dp.addTransactionInfo (beneficiaries[bPosition], amount);
                        balance = balance-amount;
                        bPosition = bPosition + 1;
                        if(bPosition >= bCount)
                        {
                            priorBalance = 0;
                            break;
                        }
                    }
                    //if balance < amount
                    bufferContracts[contractAddr].amount = balance;
                    bufferContractsIndex.push(contractAddr);
                    priorBalance = priorBalance + balance;
                    dp.updateBalance(balance);//updates balance in contract.
                    
                    //now move to next contract;
                }
                else
                {
                    address beneficiary = beneficiaries[bPosition];
                    uint remainingAmount = TransferFromPriorBalance(amount, beneficiary, dp);
                    //log to investor;
                    beneficiary.transfer(remainingAmount);
                    balance = balance - remainingAmount;
                    bPosition = bPosition + 1;
                    dp.updateBalance(balance);
                    if(priorBalance > 0)
                    {
                        //at this point there should be no balance left;
                        revert();
                    }
                }
            }
        }
        return true;
    }
    
    
    
    function TransferFromPriorBalance(uint _amount, address _beneficiary, DonationPledge _dp)
    private
    returns(uint _remainingAmount)
    {
        uint _tranAmtTotal = transferPriorContractBalance(_beneficiary, _dp);
        _remainingAmount = _amount - _tranAmtTotal;
        priorBalance = priorBalance - _tranAmtTotal;
        return _remainingAmount;
    }
    
     function transferPriorContractBalance(address _beneficiary, DonationPledge _dp)
    private
    returns(uint _tranAmtTotal)
    {
        _tranAmtTotal = 0;
        for(uint i=0; i< bufferContractsIndex.length; i++)
        {
            uint _tranAmt =  bufferContracts[bufferContractsIndex[i]].amount;
            if( _tranAmt <= 0)
                continue;
            _beneficiary.transfer(_tranAmt);
            _dp.addTransactionInfo(_beneficiary, _tranAmt);
            delete bufferContracts[bufferContractsIndex[i]];
            //logs transaction to dpContract;
            reducePledgeBalance(bufferContractsIndex[i], _tranAmt);
            //TODO:log transaction to investor;
            _tranAmtTotal = _tranAmtTotal + _tranAmt;
        }
        
        return _tranAmtTotal;
    }
    
    
    function reducePledgeBalance(address _dpAddr, uint _amount) 
    private
    {
        DonationPledge dp;
        dp = DonationPledge(_dpAddr);
        dp.reduceBalance(_amount);
    }
    
    
    
    function getBeneficiaries(string cfgName)
    private view
    returns(address[] beneficiaries)
    {
        return RegisteredCampaignFGs[cfgName].beneficiaries;
    }
    
    function getcfgContracts(string cfgName)
    public view
    returns(address[] contracts)
    {
        return RegisteredCampaignFGs[cfgName].contracts;
    }
    
    
    
    
  	//inserts new investor to the registered investors list.
    //acctId should be created before this call.
    function addInvestor(address acctId, string name, string emailId, string phNumber, string password)
    public
    returns(uint index){
        if(isUserExist(acctId,  ParticipantTypes.Investor)) revert();
        RegisteredInvestors[acctId].investorProfile.name = name;
        RegisteredInvestors[acctId].investorProfile.emailId = emailId;
        RegisteredInvestors[acctId].investorProfile.phNumber = phNumber;
        RegisteredInvestors[acctId].investorProfile.password = password;
        RegisteredInvestors[acctId].investorProfile.acctId = acctId;
        RegisteredInvestors[acctId].investorProfile.index = invIndex.push(acctId) - 1;
        return invIndex.length - 1;
    }
	
	//inserts new campaign/focus group to the ist.
    //acctId should be created before this call.
    function addCampaignFG(address fmId, string name, uint256 startDate, uint256 expiryDate)
    public
    returns(uint index){
        RegisteredCampaignFGs[name].fmId = fmId;
        RegisteredCampaignFGs[name].startDate = startDate;
        RegisteredCampaignFGs[name].expiryDate = expiryDate;
        RegisteredCampaignFGs[name].active = true;
        RegisteredCampaignFGs[name].index = cFGIndex.push(name) - 1;
        return cFGIndex.length - 1;
    }
    
	//inserts new fund manager to the registered fm list.
    //acctId should be created before this call.
    function addFM(address acctId, string name, string emailId, string phNumber, string password)
    public
    returns(uint index){
        if(isUserExist(acctId, ParticipantTypes.FM)) revert();
        RegisteredFMs[acctId].fmProfile.name = name;
        RegisteredFMs[acctId].fmProfile.emailId = emailId;
        RegisteredFMs[acctId].fmProfile.phNumber = phNumber;
        RegisteredFMs[acctId].fmProfile.password = password;
        RegisteredFMs[acctId].fmProfile.acctId = acctId;
        RegisteredFMs[acctId].fmProfile.index = fmIndex.push(acctId) - 1;
        return fmIndex.length - 1;
    }
    
    //inserts new fund manager to the registered fm list.
    //acctId should be created before this call.
    //use addBeneficiaryToCFG method to add cfg for a Beneficiary.
    function addBeneficiary(address acctId, address fmId, string name, string emailId, string phNumber, string password)
    public
    returns(uint index){
        if(isUserExist(acctId,  ParticipantTypes.Beneficiary)) revert();
        RegisteredBeneficiaries[acctId].beneProfile.name = name;
        RegisteredBeneficiaries[acctId].beneProfile.emailId = emailId;
        RegisteredBeneficiaries[acctId].beneProfile.phNumber = phNumber;
        RegisteredBeneficiaries[acctId].beneProfile.password = password;
        RegisteredBeneficiaries[acctId].beneProfile.acctId = acctId;
        RegisteredBeneficiaries[acctId].beneProfile.index = beneIndex.push(acctId) - 1;//for tracking the beneficiaries from global collection.
        RegisteredFMs[fmId].beneficiaries.push(acctId);//maps beneficiary to fm.
        return beneIndex.length - 1;
    }
    
    //adds a beneficiary to a campaign/ focus group.
    function addBeneficiaryToCFG(address beneAddr, string cFgName)
    public
    returns(bool success)
    {
        if(!isUserExist(beneAddr, ParticipantTypes.Beneficiary)) revert();
        RegisteredCampaignFGs[cFgName].beneficiaries.push(beneAddr);
        return true;
    }
    
     //adds a contracts pledged to a campaign/ focus group.
    function addContractsToCFG(address contractAddr, string cFgName)
    public
    returns(bool success)
    {
        //check if cfg exists
        RegisteredCampaignFGs[cFgName].contracts.push(contractAddr);
        return true;
    }
	
	
	//returns true if the current user is a registered user.
    function isUserExist (address curUser, ParticipantTypes pType)
    private 
    constant
    returns(bool isValid){
            if(pType == ParticipantTypes.Investor)
            {
                if(invIndex.length == 0 ) return false;
                return (invIndex[RegisteredInvestors[curUser].investorProfile.index] == curUser);
            }
            
            if(pType == ParticipantTypes.FM)
            {
                if(fmIndex.length == 0 ) return false;
                return (fmIndex[RegisteredFMs[curUser].fmProfile.index] == curUser);
            }
            
            if(pType == ParticipantTypes.Beneficiary)
            {
                if(beneIndex.length == 0 ) return false;
                return (beneIndex[RegisteredBeneficiaries[curUser].beneProfile.index] == curUser);
            }
    }
    
    //returns the registered user info if exists.
    function  getAssociatedCollections(address id, ParticipantTypes pType)
    public 
    payable
    returns (address[] collections ){
        if(!isUserExist(id,pType)) revert();
        if(pType == ParticipantTypes.Investor)
        {
            
            return( RegisteredInvestors[id].transactions);
        }
        
        if(pType == ParticipantTypes.FM)
        {
            return(RegisteredFMs[id].beneficiaries);
        }
    }
    
    //returns the registered user info if exists.
    function  getUser(address id, ParticipantTypes pType)
    public 
    payable
    returns (string name, string emailId, string phNumber, string password, address acctId,uint index){
        if(!isUserExist(id,pType)) revert();
        if(pType == ParticipantTypes.Investor)
        {
            
            return(
            RegisteredInvestors[id].investorProfile.name,
            RegisteredInvestors[id].investorProfile.emailId,
            RegisteredInvestors[id].investorProfile.phNumber,
            RegisteredInvestors[id].investorProfile.password,
            RegisteredInvestors[id].investorProfile.acctId,
            RegisteredInvestors[id].investorProfile.index
            );
        }
        
        if(pType == ParticipantTypes.FM)
        {
            return(
            RegisteredFMs[id].fmProfile.name,
            RegisteredFMs[id].fmProfile.emailId,
            RegisteredFMs[id].fmProfile.phNumber,
            RegisteredFMs[id].fmProfile.password,
            RegisteredFMs[id].fmProfile.acctId,
            RegisteredFMs[id].fmProfile.index
            );
        }
        
        if(pType == ParticipantTypes.Beneficiary)
        {
            return(
            RegisteredBeneficiaries[id].beneProfile.name,
            RegisteredBeneficiaries[id].beneProfile.emailId,
            RegisteredBeneficiaries[id].beneProfile.phNumber,
            RegisteredBeneficiaries[id].beneProfile.password,
            RegisteredBeneficiaries[id].beneProfile.acctId,
            RegisteredBeneficiaries[id].beneProfile.index
            );
        }
    }
    
     
    //end - region move user management to new contract????
    
    
    
    //creates a contract, each time an investor pledges amount for a campaign/focus group
    function CreatePledge( address fmAddr, string cFgName)
    public
    payable
    returns (address contractAddr) 
    {
        DonationPledge dp =  (new DonationPledge).value(msg.value)(address(msg.sender));
        dp.addDetails(fmAddr, cFgName);
        RegisteredInvestors[msg.sender].contracts.push(address(dp));
        RegisteredCampaignFGs[cFgName].contracts.push(address(dp));
        return address(dp);
    }
    
    
    //returns the collection of beneficiaries under an fm.
    function listBeneficiaries(address fmAddr)
    public view 
    returns (address[] beneficiaries)
    {
        return RegisteredFMs[fmAddr].beneficiaries;
    }
    
}

//Contract for donation 
contract DonationPledge {
    uint public value;
    address public investor;
    address public fundManager;
    string public campaignFG;
    mapping(address=>tranInfo) public transactionInfo; 
    address[] tranInfoIndex;
    enum State { Created, Locked, Inactive }
    State public state;
    uint public balance;
    
    struct tranInfo{
        address beneficiaryId;
        uint amountDisbursed;
    }


    function DonationPledge(address invAddr) public payable {
        investor = invAddr;
        value = msg.value;
        balance = msg.value;
        require(value > 0);
    }
     
    //adds details to the instance
    //returns the contract address
    function addDetails(address fmAddr, string cFgName)
    public
    returns (address contractAddr)
    {
        fundManager = fmAddr;
        campaignFG = cFgName;
        intialTransfer(value);
        return address(this);
    }

    function getContractDetails()
    public
    view
    returns (address , address , string , uint , uint )
    {
        return(investor, fundManager,campaignFG, value, balance);
    }
    
   
    
    function addTransactionInfo (address _beneficiary, uint _amount) public
    {
        transactionInfo[_beneficiary].beneficiaryId = _beneficiary;
        transactionInfo[_beneficiary].amountDisbursed = _amount; 
        tranInfoIndex.push(_beneficiary);
    }
    
    function intialTransfer(uint amount)
    private 
    returns(uint fmBal, uint invBal)
    {
        fundManager.transfer(amount);
        return (fundManager.balance, investor.balance);
    }
    
    function updateBalance(uint amount)
    public 
    returns(uint bal)
    {
        balance = amount;
        return (balance);
    }
    
    function reduceBalance(uint amount)
    public 
    returns(uint bal)
    {
        balance = balance -amount;
        return (balance);
    }


    modifier condition(bool _condition) {
        require(_condition);
        _;
    }

    

    modifier onlyInvestor() {
        require(msg.sender == investor);
        _;
    }

    modifier inState(State _state) {
        require(state == _state);
        _;
    }

    event Aborted();
    event PurchaseConfirmed();
    event ItemReceived();

    /// Abort the purchase and reclaim the ether.
    /// Can only be called by the seller before
    /// the contract is locked.
    function abort()
        public
        onlyInvestor
        inState(State.Created)
    {
        emit Aborted();
        state = State.Inactive;
        investor.transfer(address(this).balance);
    }
   
   
   
}