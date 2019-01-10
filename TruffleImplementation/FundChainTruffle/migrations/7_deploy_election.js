//var name should be same as that of the contract.
var Master = artifacts.require("./Master.sol");

module.exports = function(deployer) {
  deployer.deploy(Master);
};

