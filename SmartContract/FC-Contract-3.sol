pragma solidity ^0.4.18;

contract StartPayment {

    //represents the registered user.
    struct Participant{
		string name;//required
		string emailId;//required
		string phNumber;//required
		address acctId;//autogen
		string idProof;//required if the investor needs a statement, mandatory for Fm and beneficiary 
		string password;//autogen
		uint pType;//identifies the Participant as Investor =1/FM =2/Beneficiary=3
		uint index;
		bool isSignedIn;
	}
    
    //represents the current user who tries to login.
    //address public currentUser;
    
    //Collection of all registered users.
    //this in-memory array is the list of all registered users
    //against which the authentication will be validated.
    mapping(address => Participant) private RegisteredUsers;
    
    //is used to manipulate the length of the collection
    //since mapping does not support iteration.
    address[] private userIndex;
    
    //inserts new user to the registered users list.
    //acctId should be created before this call.
    function addUser(address acctId, string name, string emailId, string phNumber, string password, uint pType)
    public
    returns(uint index){
        if(isUserExist(acctId)) revert();
        RegisteredUsers[acctId].name = name;
        RegisteredUsers[acctId].emailId = emailId;
        RegisteredUsers[acctId].phNumber = phNumber;
        RegisteredUsers[acctId].password = password;
        RegisteredUsers[acctId].pType = pType;
        RegisteredUsers[acctId].acctId = acctId;
        RegisteredUsers[acctId].index = userIndex.push(acctId) - 1;
        return userIndex.length - 1;
    }
    
    //returns true if the current user is a registered user.
    function isUserExist(address curUser) private constant returns(bool isValid){
            if(userIndex.length == 0 ) return false;
            return (userIndex[RegisteredUsers[curUser].index] == curUser);
    }
    
    //returns the registered user info if exists.
    function getUser(address user) public constant returns (string name, string emailId, string phNumber, string password, uint pType, uint index ){
        if(!isUserExist(user)) revert();
        return(
            RegisteredUsers[user].name,
            RegisteredUsers[user].emailId,
            RegisteredUsers[user].phNumber,
            RegisteredUsers[user].password,
            RegisteredUsers[user].pType,
            RegisteredUsers[user].index
            );
    }
    
    //sets the sign status of the registered user.
    function updateSignInfo(address user, bool signIn)
    private
    returns(bool success){
        RegisteredUsers[user].isSignedIn = signIn;
        return true;
    }
    
    //allows the user to sign in after authentication.
    function signIn(address user)
    public
    returns(bool success)
    {
        if(!isUserExist(user)) revert();
        //string memory _name;
        bool _isSignedIn;
        //uint _index;
        //(_name,_isSignedIn, _index ) = getUser(user);
        if(!_isSignedIn){
            updateSignInfo(user, true);
        return true;
        }
    }
    
    //let the user to signout.
    function signOut(address user) public returns (bool success)
    {
       return(updateSignInfo(user, false));
    }
}