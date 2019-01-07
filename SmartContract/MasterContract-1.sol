pragma solidity ^0.4.21;

//Contract for donation 
contract Master{
    constructor() public {}
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