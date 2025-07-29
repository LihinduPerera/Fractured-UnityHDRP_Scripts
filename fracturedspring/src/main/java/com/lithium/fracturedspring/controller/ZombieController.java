package com.lithium.fracturedspring.controller;

import com.lithium.fracturedspring.model.ZombieKill;
import com.lithium.fracturedspring.service.ZombieService;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.http.ResponseEntity;
import org.springframework.stereotype.Repository;
import org.springframework.web.bind.annotation.*;

import java.util.List;

@RestController
@RequestMapping("/api/zombies")
public class ZombieController {
    private final ZombieService zombieService;

    @Autowired
    public ZombieController(ZombieService zombieService) {
        this.zombieService = zombieService;
    }

    @PostMapping("/kill")
    public ResponseEntity<ZombieKill> recordZombieKill(
            @RequestParam String zombieType,
            @RequestParam String weaponUsed
    ) {
        ZombieKill kill = zombieService.recordKill(zombieType, weaponUsed);
        return ResponseEntity.ok(kill);
    }

    @GetMapping("/count")
    public ResponseEntity<Long> getTotalKills() {
        return ResponseEntity.ok(zombieService.getTotalKills());
    }

    @GetMapping("/count/{type}")
    public ResponseEntity<Long> getKillsByType(@PathVariable String type) {
        return ResponseEntity.ok(zombieService.getKillsByType(type));
    }

    @GetMapping("/all")
    public ResponseEntity<List<ZombieKill>> getAllKills() {
        return ResponseEntity.ok(zombieService.getAllKills());
    }
}
