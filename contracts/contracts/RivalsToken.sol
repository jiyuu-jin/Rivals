// SPDX-License-Identifier: MIT
pragma solidity ^0.8.20;

import {ERC20} from "@openzeppelin/contracts/token/ERC20/ERC20.sol";

contract RivalsToken is ERC20 {
    address public _owner;
    
    constructor(address owner) ERC20("Rival", "RIVAL") {
        _owner = owner;
    }

    function spend(address playerAddress, uint256 amount) public {
        require(msg.sender == _owner, "Only owner can call spend");
        _burn(playerAddress, amount);
    }

    function killMonster(address rewardAddress) public {
        require(msg.sender == _owner, "Only owner can call killMonster");
        _mint(rewardAddress, 1000000000000000000);
    }

    function dieByMonster(address playerAddress) public {
        require(msg.sender == _owner, "Only owner can call dieByMonster");
        uint256 playerBalance = balanceOf(playerAddress);
        uint256 amountToBurn = (playerBalance * 20) / 100; // 20% of player's balance
        _burn(playerAddress, amountToBurn);
    }

    function dieByTrap(address playerAddress, address trapOwner) public {
        require(msg.sender == _owner, "Only owner can call dieByTrap");
        uint256 playerBalance = balanceOf(playerAddress);
        uint256 amountToTransfer = (playerBalance * 20) / 100; // 20% of player's balance
        _transfer(playerAddress, trapOwner, amountToTransfer);
    }
}
