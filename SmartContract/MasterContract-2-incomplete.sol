pragma solidity ^0.4.21;

//Contract for donation 
contract Master{
    constructor() public {}
    
    //start - move this to new contract????
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
	    address[] transactions;
	}
	
	struct Beneficiary{
	    Participant beneProfile;
	    address[] contracts;
	}
	
	struct FM{
	    Participant fmProfile;
	    address[] beneficiaries;
	}
	
    //Collection of all registered users.
    //this in-memory array is the list of all registered users
    //against which the authentication will be validated.
    mapping(address => Investor) private RegisteredInvestors;
    mapping(address => FM) private RegisteredFMs;
    mapping(address => Beneficiary) private RegisteredBeneficiaries;
    
    //is used to manipulate the length of the collection
    //since mapping does not support iteration.
    address[] private invIndex;
    address[] private fmIndex;
    address[] private beneIndex;
    
    //inserts new investor to the registered investors list.
    //acctId should be created before this call.
    function addInvestor(address acctId, string name, string emailId, string phNumber, string password)
    public
    returns(uint index){
        if(isUserExist(acctId, 1)) revert();
        RegisteredInvestors[acctId].investorProfile.name = name;
        RegisteredInvestors[acctId].investorProfile.emailId = emailId;
        RegisteredInvestors[acctId].investorProfile.phNumber = phNumber;
        RegisteredInvestors[acctId].investorProfile.password = password;
        RegisteredInvestors[acctId].investorProfile.acctId = acctId;
        RegisteredInvestors[acctId].investorProfile.index = invIndex.push(acctId) - 1;
        return invIndex.length - 1;
    }
	
	//inserts new fund manager to the registered fm list.
    //acctId should be created before this call.
    function addFM(address acctId, string name, string emailId, string phNumber, string password)
    public
    returns(uint index){
        if(isUserExist(acctId, 2)) revert();
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
    function addBeneficiary(address acctId, address fmId, string name, string emailId, string phNumber, string password)
    public
    returns(uint index){
        if(isUserExist(acctId, 3)) revert();
        RegisteredBeneficiaries[acctId].beneProfile.name = name;
        RegisteredBeneficiaries[acctId].beneProfile.emailId = emailId;
        RegisteredBeneficiaries[acctId].beneProfile.phNumber = phNumber;
        RegisteredBeneficiaries[acctId].beneProfile.password = password;
        RegisteredBeneficiaries[acctId].beneProfile.acctId = acctId;
        RegisteredBeneficiaries[acctId].beneProfile.index = beneIndex.push(acctId) - 1;//for tracking the beneficiaries from global collection.
        RegisteredFMs[fmId].beneficiaries.push(acctId);//maps beneficiary to fm.
        return beneIndex.length - 1;
    }
    
	//returns true if the current user is a registered user.
    function isUserExist(address curUser, uint userType) private constant returns(bool isValid){
            if(userType == 1)
            {
                if(invIndex.length == 0 ) return false;
                return (invIndex[RegisteredInvestors[curUser].investorProfile.index] == curUser);
            }
            
            if(userType == 2)
            {
                if(fmIndex.length == 0 ) return false;
                return (fmIndex[RegisteredFMs[curUser].fmProfile.index] == curUser);
            }
            
            if(userType == 3)
            {
                if(beneIndex.length == 0 ) return false;
                return (beneIndex[RegisteredBeneficiaries[curUser].beneProfile.index] == curUser);
            }
    }
    
    //returns the registered user info if exists.
    function getUser(address acctId) public constant returns (string name, string emailId, string phNumber, string password, uint pType, uint index ){
        if(!isUserExist(acctId,pType)) revert();
        if(pType == 1)
        {
            return(
            RegisteredInvestors[acctId].investorProfile.name,
            RegisteredInvestors[acctId].investorProfile.emailId,
            RegisteredInvestors[acctId].investorProfile.phNumber,
            RegisteredInvestors[acctId].investorProfile.password,
            RegisteredInvestors[acctId].investorProfile.acctId,
            RegisteredInvestors[acctId].investorProfile.index
            );
        }
        
        if(pType == 2)
        {
            return(
            RegisteredFMs[acctId].fmProfile.name,
            RegisteredFMs[acctId].fmProfile.emailId,
            RegisteredFMs[acctId].fmProfile.phNumber,
            RegisteredFMs[acctId].fmProfile.password,
            RegisteredFMs[acctId].fmProfile.acctId,
            RegisteredFMs[acctId].fmProfile.index
            );
        }
        
        if(pType == 3)
        {
            return(
            RegisteredBeneficiaries[acctId].beneProfile.name,
            RegisteredBeneficiaries[acctId].beneProfile.emailId,
            RegisteredBeneficiaries[acctId].beneProfile.phNumber,
            RegisteredBeneficiaries[acctId].beneProfile.password,
            RegisteredBeneficiaries[acctId].beneProfile.acctId,
            RegisteredBeneficiaries[acctId].beneProfile.index
            );
        }
    }
    
    //end - move this to new contract????
    
    
    
    //creates a contract, each time an investor pledges amount for a beneficiary
    function CreatePledge( address fmAddr, address benefAddr)
    public
    payable
    returns (address contractAddr) 
    {
        DonationPledge dp =  (new DonationPledge).value(msg.value)(address(msg.sender));
        dp.addDetails(fmAddr, benefAddr);
        return address(dp);
    }
    
    function getDetails(address contractAddr)
    public view
    returns (address investorAddress, address fundManagerAddr, address beneficiaryAddr, uint pledgedAmount)
    {
        DonationPledge dp;
        dp = DonationPledge(contractAddr);
        return dp.getDetails();
    }

    //core logic for fund transaction to beneficiary
    function disburseFund(address beneficiaryAddr)
    public 
    payable
    returns (address transactionAddr)
    {
        
        
    }
}

//Contract for donation 
contract DonationPledge {
    uint public value;
    address public investor;
    address public fundManager;
    address public beneficiary;
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
    
    //adds details to the instance
    //returns the contract address
    function addDetails(address fmAddr, address benefAddr)
    public
    returns (address contractAddr)
    {
        fundManager = fmAddr;
        beneficiary = benefAddr;
        return address(this);
    }

    function getDetails()
    public
    view
    returns (address investorAddress, address fundManagerAddr, address beneficiaryAddr, uint pledgedAmount)
    {
        return(investor, fundManager, beneficiary, value);
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