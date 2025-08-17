// SPDX-License-Identifier: MIT
pragma solidity ^0.8.20;

import {ERC20} from "@openzeppelin/contracts/token/ERC20/ERC20.sol";

contract RivalToken is ERC20 {
    address public _owner;
    
    constructor(address owner) ERC20("Rival", "RIVAL") {
        _owner = owner;
    }

    function killMonster(address rewardAddress, uint256 amount) public {
        require(msg.sender == _owner, "Only owner can reward for killing monsters");
        _mint(rewardAddress, amount);
    }
}
