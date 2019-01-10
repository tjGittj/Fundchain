pragma solidity ^0.4.21;
pragma experimental ABIEncoderV2;

//import "./DonationPledge.sol";

contract Master{
	uint lastIndex = 0;
    constructor() public payable {
    	//lastIndex = createUser();//creates test users
    }
    enum ParticipantTypes { Investor, FM, Beneficiary }
    uint priorBalance = 0;
	
	struct Beneficiary{
	    Participant beneProfile;
	}
	
	//contract and balance info that has balance but insufficient.
	struct bufferContract{
	    uint amount;
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

	struct FM{
	    Participant fmProfile;
	    string[] campaignFGs;
	    address[] beneficiaries;
	    mapping(address=>address) transactions;
	    uint balance;
	}
	
	struct Investor{
	    Participant investorProfile;
	    address[] transactions;// transactions happened 
	    address[] contracts;//pledged contracts
	}

    //represents the registered user.
    struct Participant{
		string name;//required
		string emailId;//required
		string phNumber;//required
		address acctId;//autogen
		string idProof;//required if the investor needs a statement, mandatory for Fm and beneficiary 
		string password;//autogen
		uint index;
	}

	//Collection of all registered users.
    mapping(address => Investor) private RegisteredInvestors;
    mapping(address => FM) private RegisteredFMs;
    mapping(address => Beneficiary) private RegisteredBeneficiaries;
    mapping(string => CampaignFG) private RegisteredCampaignFGs;//TODO : enhancement - need another key than name


    //to handle balances and transactions.
    //this collection is the contracts that have balances but insufficient.
    mapping(address=>bufferContract) bufferContracts;
    address[] private bufferContractsIndex;
    
    //is used to manipulate the length of the collection
    //since mapping does not support iteration.
    address[] private beneIndex;
    string[] private cFGIndex;
    address[] private fmIndex;
    address[] private invIndex;
    
    //inserts new fund manager to the registered fm list.
    //returns the index of the inserted beneficiary in the registered beneficiaries list.
    //use addBeneficiaryToCFG() method to add a Beneficiary to campaign.
    function addBeneficiary(address _acctId, address _fmId, string _name, string _emailId, string _phNumber, string _password)
    public
    returns(uint){
        if(isUserExist(_acctId,  ParticipantTypes.Beneficiary)) revert();
        RegisteredBeneficiaries[_acctId].beneProfile.name = _name;
        RegisteredBeneficiaries[_acctId].beneProfile.emailId = _emailId;
        RegisteredBeneficiaries[_acctId].beneProfile.phNumber = _phNumber;
        RegisteredBeneficiaries[_acctId].beneProfile.password = _password;
        RegisteredBeneficiaries[_acctId].beneProfile.acctId = _acctId;
        RegisteredBeneficiaries[_acctId].beneProfile.index = beneIndex.push(_acctId) - 1;//for tracking the beneficiaries from global collection.
        RegisteredFMs[_fmId].beneficiaries.push(_acctId);//maps beneficiary to fm.
        return beneIndex.length - 1;
    }

	//adds a beneficiary to a campaign/ focus group.
	//returns true if added successfully.
    function addBeneficiaryToCFG(address _beneAddr, string _cFgName)
    public
    returns(bool)
    {
        if(!isUserExist(_beneAddr, ParticipantTypes.Beneficiary)) revert();
        RegisteredCampaignFGs[_cFgName].beneficiaries.push(_beneAddr);
        return true;
    }

	//inserts new campaign/focus group to the ist.
    //returns the index of the inserted campaign in the registered campaigns list.
    //name is the name of the campaign.
    function addCampaignFG(string _name, address _fmId, uint256 _startDate, uint256 _expiryDate)
    public
    returns(uint){
        RegisteredCampaignFGs[_name].fmId = _fmId;
        RegisteredCampaignFGs[_name].startDate = _startDate;
        RegisteredCampaignFGs[_name].expiryDate = _expiryDate;
        RegisteredCampaignFGs[_name].active = true;
        RegisteredCampaignFGs[_name].index = cFGIndex.push(_name) - 1;
        RegisteredFMs[_fmId].campaignFGs.push(_name);
        return cFGIndex.length - 1;
    }
    
     //adds a contracts pledged to a campaign/ focus group.
     //returns true if added successfully.
    function addContractToCFG(address _contractAddr, string _cFgName)
    public
    returns(bool)
    {
        //check if cfg exists
        RegisteredCampaignFGs[_cFgName].contracts.push(_contractAddr);
        return true;
    }

    //inserts new fund manager to the registered fm list.
    //returns the index of the inserted fund manager in the registered fund managers list.
    function addFM(address _acctId, string _name, string _emailId, string _phNumber, string _password)
    public
    returns(uint){
        if(isUserExist(_acctId, ParticipantTypes.FM)) revert();
        RegisteredFMs[_acctId].fmProfile.name = _name;
        RegisteredFMs[_acctId].fmProfile.emailId = _emailId;
        RegisteredFMs[_acctId].fmProfile.phNumber = _phNumber;
        RegisteredFMs[_acctId].fmProfile.password = _password;
        RegisteredFMs[_acctId].fmProfile.acctId = _acctId;
        RegisteredFMs[_acctId].fmProfile.index = fmIndex.push(_acctId) - 1;
        return fmIndex.length - 1;
    }

    //inserts new investor to the registered investors list.
    //returns the index of the inserted user.
    function addInvestor(address _acctId, string  _name, string  _emailId, string  _phNumber,  string  _password)
    public
    returns(uint){
        if(isUserExist(_acctId,  ParticipantTypes.Investor)) revert();
        RegisteredInvestors[_acctId].investorProfile.name = _name;
        RegisteredInvestors[_acctId].investorProfile.emailId = _emailId;
        RegisteredInvestors[_acctId].investorProfile.phNumber = _phNumber;
        RegisteredInvestors[_acctId].investorProfile.password = _password;
        RegisteredInvestors[_acctId].investorProfile.acctId = _acctId;
        RegisteredInvestors[_acctId].investorProfile.index = invIndex.push(_acctId) - 1;
        return invIndex.length - 1;
    }

    //creates a contract, each time an investor pledges amount for a campaign/focus group
    function createPledge(address _invAddr, address _fmAddr, string _cFgName, uint _amount)
    public
    payable
    returns (address) 
    {
    	require(_amount > 0);
        DonationPledge dp =  new DonationPledge();
        dp.addPledgeDetails(_invAddr, _fmAddr, _cFgName, _amount);
        RegisteredInvestors[msg.sender].contracts.push(address(dp));
        RegisteredCampaignFGs[_cFgName].contracts.push(address(dp));
        return address(dp);
    }

    //returns the campaigns registered under a fund manager.
    //TODO: implement actula filteration.
    function getCampaigns()
    public view
    returns(string[])
    {
    	return cFGIndex;
    }

    //returns the campaigns registered under a fund manager.
    //TODO: implement actula filteration.
    function getCampaignsUnderFM(address _fmAddr)
    public view
    returns(string[])
    {
        return RegisteredFMs[_fmAddr].campaignFGs;
    }

    //returns the beneficiaries under a campaign.
    function getCFGBeneficiaries(string _cfgName)
    public view
    returns(address[])
    {
        return RegisteredCampaignFGs[_cfgName].beneficiaries;
    }
    
    //returns the total number of beneficiaries under a campaign.
    function getCFGBeneficiariesCount(string _cfgName)
    public view
    returns(uint256)
    {
        return RegisteredCampaignFGs[_cfgName].beneficiaries.length;
    }

    //returns the contracts pledged against a campaign.
    function getCFGContracts(string _cfgName)
    public view
    returns(address[])
    {
        return RegisteredCampaignFGs[_cfgName].contracts;
    }

    //returns the details of the contract pledged by investor.
    function getPledgeContractDetails(address _contractAddr)
    public view
    returns (address _investorAddress, address _fundManagerAddr, string _cfgName, uint _pledgedAmount, uint _balance)
    {
        DonationPledge dp;
        dp = DonationPledge(_contractAddr);
        return dp.getContractDetails();
    }

    //returns the pledged contracted information.
    //returning struct is not supported as of Nov 23, 18.
    //so beneficiaries address and amount details are returned separately.
    function getContractTransactions(address _contractAddr)
    public view
    returns(address[], uint[])
    {
        DonationPledge dp = DonationPledge(_contractAddr);
        return dp.getContractTransactions();
    }

    //returns the registered user info if exists.
    function  getUser(address _id, ParticipantTypes _pType)
    public 
    view
    returns (string, string, string, string, address,uint){
        if(!isUserExist(_id, _pType)) revert();
        if(_pType == ParticipantTypes.Investor)
        {
            
            return(
            RegisteredInvestors[_id].investorProfile.name,
            RegisteredInvestors[_id].investorProfile.emailId,
            RegisteredInvestors[_id].investorProfile.phNumber,
            RegisteredInvestors[_id].investorProfile.password,
            RegisteredInvestors[_id].investorProfile.acctId,
            RegisteredInvestors[_id].investorProfile.index
            );
        }
        
        if(_pType == ParticipantTypes.FM)
        {
            return(
            RegisteredFMs[_id].fmProfile.name,
            RegisteredFMs[_id].fmProfile.emailId,
            RegisteredFMs[_id].fmProfile.phNumber,
            RegisteredFMs[_id].fmProfile.password,
            RegisteredFMs[_id].fmProfile.acctId,
            RegisteredFMs[_id].fmProfile.index
            );
        }
        
        if(_pType == ParticipantTypes.Beneficiary)
        {
            return(
            RegisteredBeneficiaries[_id].beneProfile.name,
            RegisteredBeneficiaries[_id].beneProfile.emailId,
            RegisteredBeneficiaries[_id].beneProfile.phNumber,
            RegisteredBeneficiaries[_id].beneProfile.password,
            RegisteredBeneficiaries[_id].beneProfile.acctId,
            RegisteredBeneficiaries[_id].beneProfile.index
            );
        }
    }

    //returns true if the current user is a registered user.
    function isUserExist (address _curUser, ParticipantTypes _pType)
    private 
    constant
    returns(bool){
            if(_pType == ParticipantTypes.Investor)
            {
                if(invIndex.length == 0 ) return false;
                return (invIndex[RegisteredInvestors[_curUser].investorProfile.index] == _curUser);
            }
            
            if(_pType == ParticipantTypes.FM)
            {
                if(fmIndex.length == 0 ) return false;
                return (fmIndex[RegisteredFMs[_curUser].fmProfile.index] == _curUser);
            }
            
            if(_pType == ParticipantTypes.Beneficiary)
            {
                if(beneIndex.length == 0 ) return false;
                return (beneIndex[RegisteredBeneficiaries[_curUser].beneProfile.index] == _curUser);
            }
    }


    /*//test region
    Sample data
    {
    	 app.createPledge(0xeE4517058Ff234A12700153d22e887B0CCff263D, 0xd79b66B72292c42729b22AA55231960510159F7C, CFG1", 10)
    	 app.addInvestor(0xeE4517058Ff234A12700153d22e887B0CCff263D, "Investor1", "inv1@gmail.com", "90371099873", "Pass@123")
    	 app.getUser(0xeE4517058Ff234A12700153d22e887B0CCff263D, 0)
    	 app.addFM(0xd79b66B72292c42729b22AA55231960510159F7C, "FM1", "fm1@gmail.com", "90371099873", "Pass@123")
    	 app.getUser(0xd79b66B72292c42729b22AA55231960510159F7C, 1)
    	 app.addBeneficiary(0x8F2594666D90923F40d7e32cBF059A3d92d377C1, 0xd79b66B72292c42729b22AA55231960510159F7C,"Bene1", "bene1@gmail.com", "90371099873", "Pass@123")
    	 app.getUser(0x8F2594666D90923F40d7e32cBF059A3d92d377C1, 2)
    	 app.addCampaignFG("CmFG1", 0xd79b66B72292c42729b22AA55231960510159F7C, 18, 20)
    	 app.addBeneficiaryToCFG(0x8F2594666D90923F40d7e32cBF059A3d92d377C1, "CFG1")
    	 return true;
    }*/	
}	

/**************************************************************************************************/
/* Donation Pledge contract */
pragma solidity ^0.4.21;

//Contract for donation 
contract DonationPledge {
    uint public donation;
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


    constructor()
    public
    {
        
    }
     
    //adds details to the instance
    //TRANSFERS Fund to Fund Manager
    //returns the contract address
    function addPledgeDetails(address _invAddr, address _fmAddr, string _cFgName, uint _amount)
    public
    returns (address)
    {
    	require(_amount > 0);
        donation = _amount;
        balance = _amount;
        investor = _invAddr;
        fundManager = _fmAddr;
        campaignFG = _cFgName;
        initialTransfer(donation);
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
        return(investor, fundManager,campaignFG, donation, balance);
    }
    
    //gets all the transactions performed against this contract.
    function getContractTransactions()
    public view
    returns(address[], uint[] )
    {
        address[] memory _beneficiaries;
        uint[] memory _tranAmt;
        for(uint i = 0; i < tranInfoIndex.length; i++)
        {
            tranInfo memory info = transactionInfo[tranInfoIndex[i]];
            _beneficiaries[i] = info.beneficiaryId;
            _tranAmt[i] = info.amountDisbursed;
        }
        
        return (_beneficiaries, _tranAmt);
    }
    
    //transfers the amount pledged by investor to the fund manager account
    //while creating the pledging.    
    function initialTransfer(uint _amount)
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