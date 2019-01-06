pragma solidity ^0.4.18;

contract ConcurrencyContract {

	mapping(address => uint256) public maxCount;
	mapping(address => uint256) public currentCount;	
	uint256 balance;
	
	event Transaction(uint256 currentCount, address indexed user);

	event Set(bool setResult, address indexed user);

	function getConcurrencyBalance(address accountId) public returns (uint256) {
        balance = maxCount[accountId] - currentCount[accountId];
		return balance;
    }

	function setConcurrency(address accountId, uint256 count) public returns (bool){
        	maxCount[accountId] = count;
		currentCount[accountId] = 0;
		Set(true, accountId);
		return true;
    }	

    function logout(address accountId) public returns (bool) {
	    if(currentCount[accountId] > 0)
		{
			currentCount[accountId] -= 1;
			Transaction(currentCount[accountId],accountId);
			return true;
		}
		return false;
    }

    function login(address accountId) public returns (bool) {
		if(maxCount[accountId] == 0)
			return false;
	
		if(maxCount[accountId] > currentCount[accountId])
		{
			currentCount[accountId] += 1;
			Transaction(currentCount[accountId],accountId);
			return true;
		}        
		return false;
    }
}