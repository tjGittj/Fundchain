//var name should be same as that of the contract.
//create migration file every time of deployment.
//increase the number prefixed to the file name.
//More Info http://www.dappuniversity.com/articles/the-ultimate-ethereum-dapp-tutorial
var Master = artifacts.require("./Master.sol");

module.exports = function(deployer) {
  deployer.deploy(Master);
};

