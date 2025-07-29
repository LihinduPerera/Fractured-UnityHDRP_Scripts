package com.lithium.fracturedspring.service;

import com.lithium.fracturedspring.model.ZombieKill;
import com.lithium.fracturedspring.repository.ZombieKillRepository;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.stereotype.Service;


import java.util.List;

@Service
public class ZombieService {
    private final ZombieKillRepository zombieKillRepository;

    @Autowired
    public ZombieService(ZombieKillRepository zombieKillRepository) {
        this.zombieKillRepository = zombieKillRepository;
    }

    public ZombieKill recordKill(String zombieType, String weaponUsed) {
        ZombieKill kill = new ZombieKill(zombieType, weaponUsed);
        return zombieKillRepository.save(kill);
    }

    public long getTotalKills() {
        return zombieKillRepository.countAllBy();
    }

    public long getKillsByType(String zombieType) {
        return zombieKillRepository.countByZombieType(zombieType);
    }

    public List<ZombieKill> getAllKills() {
        return zombieKillRepository.findAll();
    }
}
