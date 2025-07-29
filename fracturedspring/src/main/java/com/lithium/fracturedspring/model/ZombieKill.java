package com.lithium.fracturedspring.model;

import jakarta.persistence.Entity;
import jakarta.persistence.GeneratedValue;
import jakarta.persistence.GenerationType;
import jakarta.persistence.Id;

import java.time.LocalDateTime;

@Entity
public class ZombieKill {
    @Id
    @GeneratedValue(strategy = GenerationType.IDENTITY)
    private Long id;

    private String zombieType;
    private LocalDateTime killTime;
    private String weaponUsed;

    public ZombieKill() {
        this.killTime = LocalDateTime.now();
    }

    public ZombieKill(String zombieType, String weaponUsed) {
        this.zombieType = zombieType;
        this.weaponUsed = weaponUsed;
        this.killTime = LocalDateTime.now();
    }

    public Long getId() {
        return id;
    }

    public String getZombieType() {
        return zombieType;
    }

    public void setZombieType(String zombieType) {
        this.zombieType = zombieType;
    }

    public LocalDateTime getKillTime() {
        return killTime;
    }

    public void setKillTime(LocalDateTime killTime) {
        this.killTime = killTime;
    }

    public String getWeaponUsed() {
        return weaponUsed;
    }

    public void setWeaponUsed(String weaponUsed) {
        this.weaponUsed = weaponUsed;
    }
}
