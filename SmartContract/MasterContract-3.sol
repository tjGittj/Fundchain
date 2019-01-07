pragma solidity ^0.4.21;

//Contract for donation 
contract Master{
    constructor() public payable {}
    enum ParticipantTypes { Investor, FM, Beneficiary }
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
	}
	
	struct CampaignFG{
	    string name;
	    address[] beneficiaries;
	    address[] contracts;
	    address fmId;
	    uint index;
	}
	
    //Collection of all registered users.
    //this in-memory array is the list of all registered users
    //against which the authentication will be validated.
    mapping(address => Investor) private RegisteredInvestors;
    mapping(address => FM) private RegisteredFMs;
    mapping(address => Beneficiary) private RegisteredBeneficiaries;
    mapping(string => CampaignFG) private RegisteredCampaignFGs;//need another key?
    
    //is used to manipulate the length of the collection
    //since mapping does not support iteration.
    address[] private invIndex;
    address[] private fmIndex;
    address[] private beneIndex;
    string[] private cFGIndex;
    
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
    function addCampaignFG(address fmId, string name)
    public
    returns(uint index){
        RegisteredCampaignFGs[name].fmId = fmId;
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
    
    //returns the details of the contract pledged by investor.
    function getDetails(address contractAddr)
    public view
    returns (address investorAddress, address fundManagerAddr, string cfgName, uint pledgedAmount)
    {
        DonationPledge dp;
        dp = DonationPledge(contractAddr);
        return dp.getContractDetails();
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
    address public beneficiary;//remove this 
    enum State { Created, Locked, Inactive }
    State public state;

    // Ensure that `msg.value` is an even number.
    // Division will truncate if it is an odd number.
    // Check via multiplication that it wasn't an odd number.
    constructor(address invAddr) public payable {
        investor = invAddr;
        value = msg.value;
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
    returns (address investorAddress, address fundManagerAddr, string cfgName, uint pledgedAmount)
    {
        return(investor, fundManager,cfgName, value);
    }
    
    function intialTransfer(uint amount)
    private 
    returns(uint fmBal, uint invBal)
    {
        fundManager.transfer(amount);
        return (fundManager.balance, investor.balance);
    }
    
    function transfer(uint amount)
    public 
    returns(uint fmBal, uint invBal)
    {
        if(value < amount) revert();
        fundManager.transfer(amount);
        value = value-amount;
        return (fundManager.balance, investor.balance);
    }
    



    modifier condition(bool _condition) {
        require(_condition);
        _;
    }

    modifier onlyBeneficiary() {
        require(msg.sender == beneficiary);
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
   
    /// Confirm the purchase as buyer.
    /// Transaction has to include `2 * value` ether.
    /// The ether will be locked until confirmReceived
    /// is called.
    function confirmPurchase()
        public
        inState(State.Created)
        condition(msg.value == (2 * value))
        payable
    {
        emit PurchaseConfirmed();
        beneficiary = msg.sender;
        state = State.Locked;
    }

    /// Confirm that you (the buyer) received the item.
    /// This will release the locked ether.
    function confirmReceived()
        public
        onlyBeneficiary
        inState(State.Locked)
    {
        emit ItemReceived();
        // It is important to change the state first because
        // otherwise, the contracts called using `send` below
        // can call in again here.
        state = State.Inactive;

        // NOTE: This actually allows both the buyer and the seller to
        // block the refund - the withdraw pattern should be used.

        beneficiary.transfer(value);
        investor.transfer(address(this).balance);
    }
}