package com.lithium.fracturedspring.repository;

import com.lithium.fracturedspring.model.ZombieKill;
import org.springframework.data.jpa.repository.JpaRepository;
import org.springframework.stereotype.Repository;

@Repository
public interface ZombieKillRepository extends JpaRepository<ZombieKill, Long> {
    long countByZombieType(String zombieType);
    long countAllBy();
}
