// ============================================================================
// AR 3D FRUIT SLASHER - INTERACTIVE WEB SIMULATOR
// Mirrors all Unity C# mechanics: 360° expanding arc, rear alert, blade physics, combos
// ============================================================================

// --- Web Audio API Procedural Sound Engine ---
class SoundEngine {
  constructor() {
    this.ctx = null;
    this.init();
  }

  init() {
    const AudioContext = window.AudioContext || window.webkitAudioContext;
    if (AudioContext) {
      this.ctx = new AudioContext();
    }
  }

  ensureContext() {
    if (this.ctx && this.ctx.state === 'suspended') {
      this.ctx.resume();
    }
  }

  playSwish() {
    if (!this.ctx) return;
    this.ensureContext();
    const osc = this.ctx.createOscillator();
    const gain = this.ctx.createGain();
    osc.type = 'sine';
    const now = this.ctx.currentTime;
    osc.frequency.setValueAtTime(600, now);
    osc.frequency.exponentialRampToValueAtTime(150, now + 0.12);
    gain.gain.setValueAtTime(0.3, now);
    gain.gain.linearRampToValueAtTime(0.01, now + 0.12);
    osc.connect(gain);
    gain.connect(this.ctx.destination);
    osc.start(now);
    osc.stop(now + 0.12);
  }

  playSlice() {
    if (!this.ctx) return;
    this.ensureContext();
    const now = this.ctx.currentTime;
    const osc = this.ctx.createOscillator();
    const gain = this.ctx.createGain();
    osc.type = 'triangle';
    osc.frequency.setValueAtTime(900, now);
    osc.frequency.exponentialRampToValueAtTime(180, now + 0.18);
    gain.gain.setValueAtTime(0.6, now);
    gain.gain.exponentialRampToValueAtTime(0.01, now + 0.18);
    osc.connect(gain);
    gain.connect(this.ctx.destination);
    osc.start(now);
    osc.stop(now + 0.18);
  }

  playAlert() {
    if (!this.ctx) return;
    this.ensureContext();
    const now = this.ctx.currentTime;
    [0, 0.12].forEach((offset) => {
      const osc = this.ctx.createOscillator();
      const gain = this.ctx.createGain();
      osc.type = 'sine';
      osc.frequency.setValueAtTime(1400, now + offset);
      gain.gain.setValueAtTime(0.4, now + offset);
      gain.gain.exponentialRampToValueAtTime(0.01, now + offset + 0.08);
      osc.connect(gain);
      gain.connect(this.ctx.destination);
      osc.start(now + offset);
      osc.stop(now + offset + 0.08);
    });
  }

  playExplosion() {
    if (!this.ctx) return;
    this.ensureContext();
    const now = this.ctx.currentTime;
    const osc = this.ctx.createOscillator();
    const gain = this.ctx.createGain();
    osc.type = 'sawtooth';
    osc.frequency.setValueAtTime(120, now);
    osc.frequency.exponentialRampToValueAtTime(25, now + 0.45);
    gain.gain.setValueAtTime(0.8, now);
    gain.gain.exponentialRampToValueAtTime(0.01, now + 0.45);
    osc.connect(gain);
    gain.connect(this.ctx.destination);
    osc.start(now);
    osc.stop(now + 0.45);
  }

  playCheckpoint() {
    if (!this.ctx) return;
    this.ensureContext();
    const now = this.ctx.currentTime;
    const notes = [523.25, 659.25, 783.99, 1046.5]; // C5, E5, G5, C6
    notes.forEach((freq, idx) => {
      const osc = this.ctx.createOscillator();
      const gain = this.ctx.createGain();
      osc.type = 'sine';
      osc.frequency.setValueAtTime(freq, now + idx * 0.1);
      gain.gain.setValueAtTime(0.35, now + idx * 0.1);
      gain.gain.exponentialRampToValueAtTime(0.01, now + idx * 0.1 + 0.25);
      osc.connect(gain);
      gain.connect(this.ctx.destination);
      osc.start(now + idx * 0.1);
      osc.stop(now + idx * 0.1 + 0.25);
    });
  }

  playMiss() {
    if (!this.ctx) return;
    this.ensureContext();
    const now = this.ctx.currentTime;
    const osc = this.ctx.createOscillator();
    const gain = this.ctx.createGain();
    osc.type = 'sine';
    osc.frequency.setValueAtTime(220, now);
    osc.frequency.linearRampToValueAtTime(90, now + 0.25);
    gain.gain.setValueAtTime(0.3, now);
    gain.gain.linearRampToValueAtTime(0.01, now + 0.25);
    osc.connect(gain);
    gain.connect(this.ctx.destination);
    osc.start(now);
    osc.stop(now + 0.25);
  }
}

// --- Main Game Implementation ---
class ARFruitSlasherGame {
  constructor() {
    this.audio = new SoundEngine();

    // Game State
    this.score = 0;
    this.bestScore = parseInt(localStorage.getItem('AR_FRUIT_BEST') || '0', 10);
    this.lives = 3;
    this.isGameOver = false;

    // Difficulty Tiers (100° -> 180° -> 270° -> 360°)
    this.tiers = [
      { minScore: 0,   arc: 100, interval: 2.0, name: "Level 1: 100° (Front)", bombChance: 0.0 },
      { minScore: 100, arc: 180, interval: 1.6, name: "Level 2: 180° (Wide)",  bombChance: 0.1 },
      { minScore: 200, arc: 270, interval: 1.25, name: "Level 3: 270° (Deep)", bombChance: 0.15 },
      { minScore: 300, arc: 360, interval: 0.9, name: "Level 4: 360° (Surround)", bombChance: 0.22 }
    ];
    this.currentTierIndex = 0;

    // Combo system
    this.comboStreak = 0;
    this.lastSliceTime = 0;
    this.comboResetTime = 400; // ms

    // Active Objects
    this.fruits = [];
    this.halves = [];
    this.particles = [];

    // Camera Pan (Simulating AR looking around)
    this.cameraYaw = 0; // horizontal angle in radians
    this.isPanning = false;
    this.lastPanX = 0;

    // Blade Swipe
    this.isSwiping = false;
    this.bladePoints = []; // 2D trail points: { x, y, time }
    this.previousScreenPoint = null;

    // Active Rear Threat
    this.activeThreat = null; // { pos, expireTime }

    this.initThree();
    this.initBladeCanvas();
    this.initUI();
    this.initEvents();
    this.startSpawnLoop();
    this.animate();
  }

  initThree() {
    this.threeCanvas = document.getElementById('three-canvas');
    this.renderer = new THREE.WebGLRenderer({ canvas: this.threeCanvas, antialias: true, alpha: true });
    this.renderer.setSize(window.innerWidth, window.innerHeight);
    this.renderer.setPixelRatio(Math.min(window.devicePixelRatio, 2));

    this.scene = new THREE.Scene();

    // AR Passthrough dark ambient background
    this.scene.background = new THREE.Color(0x0e1118);

    // Camera at simulated eye level
    this.camera = new THREE.PerspectiveCamera(65, window.innerWidth / window.innerHeight, 0.1, 100);
    this.camera.position.set(0, 1.4, 0);

    // Lights
    const ambientLight = new THREE.AmbientLight(0xffffff, 0.7);
    this.scene.add(ambientLight);

    const sunLight = new THREE.DirectionalLight(0xfffaed, 1.2);
    sunLight.position.set(5, 10, 5);
    this.scene.add(sunLight);

    // Futuristic AR Reticle Ground Floor (Grid to show depth in 3D AR space)
    const grid = new THREE.GridHelper(12, 24, 0x00f2fe, 0x1f293d);
    grid.position.y = -0.05;
    this.scene.add(grid);

    // Circular ground ring
    const ringGeo = new THREE.RingGeometry(2.4, 2.5, 36);
    const ringMat = new THREE.MeshBasicMaterial({ color: 0x00f2fe, side: THREE.DoubleSide, transparent: true, opacity: 0.25 });
    const ring = new THREE.Mesh(ringGeo, ringMat);
    ring.rotation.x = -Math.PI / 2;
    this.scene.add(ring);

    this.raycaster = new THREE.Raycaster();
  }

  initBladeCanvas() {
    this.bladeCanvas = document.getElementById('blade-canvas');
    this.bladeCtx = this.bladeCanvas.getContext('2d');
    this.resizeBladeCanvas();
  }

  resizeBladeCanvas() {
    this.bladeCanvas.width = window.innerWidth;
    this.bladeCanvas.height = window.innerHeight;
  }

  initUI() {
    this.scoreDisplay = document.getElementById('score-display');
    this.arcDisplay = document.getElementById('arc-display');
    this.livesDisplay = document.getElementById('lives-display');
    this.rearAlert = document.getElementById('rear-alert');
    this.alertTitle = document.getElementById('alert-title');
    this.alertArrow = document.getElementById('alert-arrow');
    this.comboBanner = document.getElementById('combo-banner');
    this.comboTitle = document.getElementById('combo-title');
    this.comboPoints = document.getElementById('combo-points');
    this.checkpointBanner = document.getElementById('checkpoint-banner');
    this.checkpointSub = document.getElementById('checkpoint-sub');
    this.scorePopups = document.getElementById('score-popups');

    this.gameOverModal = document.getElementById('game-over-modal');
    this.gameOverReason = document.getElementById('game-over-reason');
    this.finalScoreEl = document.getElementById('final-score');
    this.bestScoreEl = document.getElementById('best-score');
    this.restartBtn = document.getElementById('restart-btn');

    this.restartBtn.addEventListener('click', () => this.restartGame());
  }

  initEvents() {
    window.addEventListener('resize', () => {
      this.camera.aspect = window.innerWidth / window.innerHeight;
      this.camera.updateProjectionMatrix();
      this.renderer.setSize(window.innerWidth, window.innerHeight);
      this.resizeBladeCanvas();
    });

    // Keyboard rotation (A/D or Left/Right keys) to simulate turning in AR
    window.addEventListener('keydown', (e) => {
      if (e.key === 'a' || e.key === 'ArrowLeft') {
        this.cameraYaw += 0.15;
      } else if (e.key === 'd' || e.key === 'ArrowRight') {
        this.cameraYaw -= 0.15;
      }
    });

    // Mouse / Touch Controls
    window.addEventListener('mousedown', (e) => {
      if (e.button === 0) {
        // Left click = Blade Swipe
        this.isSwiping = true;
        this.audio.playSwish();
        this.recordBladePoint(e.clientX, e.clientY);
      } else if (e.button === 2) {
        // Right click = Pan camera (simulate AR looking around)
        this.isPanning = true;
        this.lastPanX = e.clientX;
      }
    });

    window.addEventListener('mousemove', (e) => {
      if (this.isSwiping) {
        this.recordBladePoint(e.clientX, e.clientY);
        this.checkBladeIntersection(this.previousScreenPoint, { x: e.clientX, y: e.clientY });
        this.previousScreenPoint = { x: e.clientX, y: e.clientY };
      }
      if (this.isPanning) {
        const deltaX = e.clientX - this.lastPanX;
        this.cameraYaw -= deltaX * 0.005;
        this.lastPanX = e.clientX;
      }
    });

    window.addEventListener('mouseup', (e) => {
      if (e.button === 0) {
        this.isSwiping = false;
        this.previousScreenPoint = null;
      } else if (e.button === 2) {
        this.isPanning = false;
      }
    });

    window.addEventListener('contextmenu', (e) => e.preventDefault());

    // Mobile Touch
    window.addEventListener('touchstart', (e) => {
      if (e.touches.length === 1) {
        this.isSwiping = true;
        this.audio.playSwish();
        const t = e.touches[0];
        this.recordBladePoint(t.clientX, t.clientY);
      }
    });

    window.addEventListener('touchmove', (e) => {
      if (this.isSwiping && e.touches.length === 1) {
        const t = e.touches[0];
        this.recordBladePoint(t.clientX, t.clientY);
        if (this.previousScreenPoint) {
          this.checkBladeIntersection(this.previousScreenPoint, { x: t.clientX, y: t.clientY });
        }
        this.previousScreenPoint = { x: t.clientX, y: t.clientY };
      }
    });

    window.addEventListener('touchend', () => {
      this.isSwiping = false;
      this.previousScreenPoint = null;
    });

    // Mobile Device Orientation (Real AR Gyroscope!)
    if (window.DeviceOrientationEvent) {
      window.addEventListener('deviceorientation', (e) => {
        if (e.alpha !== null) {
          this.cameraYaw = THREE.MathUtils.degToRad(-e.alpha);
        }
      });
    }
  }

  recordBladePoint(x, y) {
    this.bladePoints.push({ x, y, time: performance.now() });
    if (!this.previousScreenPoint) {
      this.previousScreenPoint = { x, y };
    }
  }

  // --- Spawner with Dynamic Angle Scaling ---
  startSpawnLoop() {
    const loop = () => {
      if (!this.isGameOver) {
        const currentTier = this.tiers[this.currentTierIndex];
        this.spawnFruit(currentTier);

        // Sometimes spawn 2 fruits at high difficulty
        if (currentTier.arc >= 270 && Math.random() < 0.35) {
          setTimeout(() => {
            if (!this.isGameOver) this.spawnFruit(currentTier);
          }, 180);
        }

        setTimeout(loop, currentTier.interval * 1000);
      }
    };
    setTimeout(loop, 1200);
  }

  spawnFruit(tier) {
    // Calculate spawn angle within the current tier's arc
    const halfArc = (tier.arc * 0.5) * (Math.PI / 180);
    const spawnAngleRel = (Math.random() * 2 - 1) * halfArc;
    const globalAngle = this.cameraYaw + spawnAngleRel;

    const radius = 2.4 + Math.random() * 0.8;
    const spawnX = Math.sin(globalAngle) * radius;
    const spawnZ = -Math.cos(globalAngle) * radius;
    const spawnY = -0.1; // Ground level

    // Check if behind or off-screen (> 55° from camera heading)
    const angleFromForward = Math.abs(spawnAngleRel) * (180 / Math.PI);
    const isBehind = angleFromForward > 55;

    // Check if bomb
    const isBomb = Math.random() < tier.bombChance;

    let mesh, fruitData;
    if (isBomb) {
      const geo = new THREE.SphereGeometry(0.24, 16, 16);
      const mat = new THREE.MeshStandardMaterial({ color: 0x18181c, roughness: 0.4 });
      mesh = new THREE.Mesh(geo, mat);

      // Yellow fuse
      const fuseGeo = new THREE.CylinderGeometry(0.04, 0.04, 0.14, 8);
      const fuseMat = new THREE.MeshBasicMaterial({ color: 0xffcc00 });
      const fuse = new THREE.Mesh(fuseGeo, fuseMat);
      fuse.position.y = 0.25;
      mesh.add(fuse);

      fruitData = { isBomb: true, points: 0, color: 0xff3300 };
    } else {
      const fruitCatalog = [
        { name: 'Watermelon', outerColor: 0x1f8f30, innerColor: 0xe6223a, size: 0.32, points: 10 },
        { name: 'Orange', outerColor: 0xff7b00, innerColor: 0xffa200, size: 0.25, points: 15 },
        { name: 'Apple', outerColor: 0xcc1824, innerColor: 0xf7f4d5, size: 0.22, points: 20 },
        { name: 'Pineapple', outerColor: 0xffc400, innerColor: 0xffdd44, size: 0.30, points: 50 }
      ];
      const cfg = fruitCatalog[Math.floor(Math.random() * fruitCatalog.length)];

      const geo = new THREE.SphereGeometry(cfg.size, 18, 18);
      const mat = new THREE.MeshStandardMaterial({ color: cfg.outerColor, roughness: 0.5 });
      mesh = new THREE.Mesh(geo, mat);

      fruitData = { isBomb: false, points: cfg.points, color: cfg.innerColor, cfg };
    }

    mesh.position.set(spawnX, spawnY, spawnZ);

    // Upward launch velocity + inward arc impulse toward center
    const upwardSpeed = 4.8 + Math.random() * 0.6;
    const inwardDir = new THREE.Vector3(-spawnX, 0, -spawnZ).normalize().multiplyScalar(1.0);
    const velocity = new THREE.Vector3(inwardDir.x, upwardSpeed, inwardDir.z);

    const spin = new THREE.Vector3(
      (Math.random() - 0.5) * 6,
      (Math.random() - 0.5) * 6,
      (Math.random() - 0.5) * 6
    );

    this.scene.add(mesh);
    this.fruits.push({ mesh, velocity, spin, data: fruitData, isSliced: false });

    // Trigger rear threat warning alert if off-screen/behind!
    if (isBehind) {
      this.triggerRearAlert(new THREE.Vector3(spawnX, spawnY, spawnZ));
    }
  }

  triggerRearAlert(pos) {
    this.activeThreat = {
      pos,
      expireTime: performance.now() + 2200
    };
    this.rearAlert.classList.remove('hidden');
    this.audio.playAlert();
  }

  updateRearAlert() {
    if (!this.activeThreat) return;

    if (performance.now() > this.activeThreat.expireTime) {
      this.rearAlert.classList.add('hidden');
      this.activeThreat = null;
      return;
    }

    // Calculate angle relative to camera forward
    const camDir = new THREE.Vector3(0, 0, -1).applyAxisAngle(new THREE.Vector3(0, 1, 0), this.cameraYaw);
    const toThreat = new THREE.Vector3(this.activeThreat.pos.x, 0, this.activeThreat.pos.z).normalize();

    const dot = camDir.dot(toThreat);
    const angleDeg = Math.acos(Math.max(-1, Math.min(1, dot))) * (180 / Math.PI);

    // If player turned and is now facing the threat
    if (angleDeg < 42) {
      this.rearAlert.classList.add('hidden');
      this.activeThreat = null;
      return;
    }

    const cross = camDir.clone().cross(toThreat);
    const isRight = cross.y < 0;
    const isDirectlyBehind = angleDeg > 115;

    if (isDirectlyBehind) {
      this.alertTitle.innerText = "BEHIND YOU! ⚠️";
      this.alertArrow.innerText = isRight ? "TURN RIGHT (BEHIND!) ↪️" : "TURN LEFT (BEHIND!) ↩️";
    } else {
      this.alertTitle.innerText = "INCOMING OBJECT!";
      this.alertArrow.innerText = isRight ? "LOOK RIGHT ➡️" : "LOOK LEFT ⬅️";
    }
  }

  // --- Slicing Intersection Detection ---
  checkBladeIntersection(p1, p2) {
    if (!p1 || !p2) return;

    const samples = 4;
    for (let i = 0; i <= samples; i++) {
      const sx = p1.x + (p2.x - p1.x) * (i / samples);
      const sy = p1.y + (p2.y - p1.y) * (i / samples);

      const mouse = new THREE.Vector2(
        (sx / window.innerWidth) * 2 - 1,
        -(sy / window.innerHeight) * 2 + 1
      );

      this.raycaster.setFromCamera(mouse, this.camera);
      const targets = this.fruits.filter(f => !f.isSliced).map(f => f.mesh);
      const intersects = this.raycaster.intersectObjects(targets);

      if (intersects.length > 0) {
        const hitMesh = intersects[0].object;
        const fruitObj = this.fruits.find(f => f.mesh === hitMesh);
        if (fruitObj && !fruitObj.isSliced) {
          const swipeDir = new THREE.Vector2(p2.x - p1.x, p2.y - p1.y).normalize();
          this.sliceFruit(fruitObj, intersects[0].point, swipeDir);
          break;
        }
      }
    }
  }

  sliceFruit(fruitObj, contactPoint, swipeDir) {
    fruitObj.isSliced = true;

    // Remove whole mesh
    this.scene.remove(fruitObj.mesh);

    if (fruitObj.data.isBomb) {
      // Explode bomb
      this.audio.playExplosion();
      this.spawnBombExplosion(contactPoint);
      this.triggerGameOver("BOOM! You sliced a Bomb!");
      return;
    }

    // Normal fruit sliced
    this.audio.playSlice();

    // Register combo
    const now = performance.now();
    if (now - this.lastSliceTime < this.comboResetTime) {
      this.comboStreak++;
    } else {
      this.finishCombo();
      this.comboStreak = 1;
    }
    this.lastSliceTime = now;

    // Add base score
    this.addScore(fruitObj.data.points);

    // Spawn 2 split halves flying apart
    this.spawnFruitHalves(fruitObj, contactPoint, swipeDir);

    // Spawn juice splatter particles
    this.spawnJuiceSplatter(contactPoint, fruitObj.data.color);

    // Spawn 3D floating score popup
    this.spawnScorePopup(contactPoint, `+${fruitObj.data.points}`);
  }

  spawnFruitHalves(fruitObj, pos, swipeDir) {
    const size = fruitObj.data.cfg ? fruitObj.data.cfg.size : 0.28;
    const halfGeo = new THREE.SphereGeometry(size, 16, 16, 0, Math.PI);

    const outerMat = new THREE.MeshStandardMaterial({ color: fruitObj.data.cfg.outerColor, roughness: 0.5 });
    const innerMat = new THREE.MeshStandardMaterial({ color: fruitObj.data.color, roughness: 0.7 });

    const pushX = swipeDir.y;
    const pushZ = -swipeDir.x;

    [-1, 1].forEach((sign) => {
      const halfMesh = new THREE.Mesh(halfGeo, outerMat);
      halfMesh.position.copy(pos);
      halfMesh.rotation.y = sign > 0 ? 0 : Math.PI;

      const vel = fruitObj.velocity.clone().multiplyScalar(0.7);
      vel.x += pushX * sign * 1.8;
      vel.z += pushZ * sign * 1.8;
      vel.y += 1.2;

      const spin = new THREE.Vector3(
        (Math.random() - 0.5) * 12,
        (Math.random() - 0.5) * 12,
        (Math.random() - 0.5) * 12
      );

      this.scene.add(halfMesh);
      this.halves.push({ mesh: halfMesh, velocity: vel, spin, spawnTime: performance.now() });
    });
  }

  spawnJuiceSplatter(pos, colorHex) {
    const count = 28;
    const pGeo = new THREE.SphereGeometry(0.045, 6, 6);
    const pMat = new THREE.MeshBasicMaterial({ color: colorHex });

    for (let i = 0; i < count; i++) {
      const p = new THREE.Mesh(pGeo, pMat);
      p.position.copy(pos);

      const vel = new THREE.Vector3(
        (Math.random() - 0.5) * 4.5,
        Math.random() * 3.5 + 0.5,
        (Math.random() - 0.5) * 4.5
      );

      this.scene.add(p);
      this.particles.push({ mesh: p, velocity: vel, spawnTime: performance.now() });
    }
  }

  spawnBombExplosion(pos) {
    const count = 50;
    const pGeo = new THREE.SphereGeometry(0.08, 6, 6);
    const pMat = new THREE.MeshBasicMaterial({ color: 0xff3300 });

    for (let i = 0; i < count; i++) {
      const p = new THREE.Mesh(pGeo, pMat);
      p.position.copy(pos);

      const vel = new THREE.Vector3(
        (Math.random() - 0.5) * 8.0,
        Math.random() * 6.0,
        (Math.random() - 0.5) * 8.0
      );

      this.scene.add(p);
      this.particles.push({ mesh: p, velocity: vel, spawnTime: performance.now() });
    }
  }

  spawnScorePopup(worldPos, text) {
    const screenPos = worldPos.clone().project(this.camera);
    const x = (screenPos.x * 0.5 + 0.5) * window.innerWidth;
    const y = (-screenPos.y * 0.5 + 0.5) * window.innerHeight;

    const popup = document.createElement('div');
    popup.className = 'floating-popup';
    popup.innerText = text;
    popup.style.left = `${x}px`;
    popup.style.top = `${y}px`;

    this.scorePopups.appendChild(popup);
    setTimeout(() => popup.remove(), 750);
  }

  finishCombo() {
    if (this.comboStreak >= 2) {
      const bonus = this.comboStreak * 20;
      this.addScore(bonus);

      const title = this.comboStreak >= 4 ? "ULTRA COMBO!" : this.comboStreak === 3 ? "TRIPLE COMBO!" : "DOUBLE SLICE!";
      this.comboTitle.innerText = `${title} (${this.comboStreak}x)`;
      this.comboPoints.innerText = `+${bonus} PTS`;

      this.comboBanner.classList.remove('hidden');
      setTimeout(() => this.comboBanner.classList.add('hidden'), 1200);
    }
    this.comboStreak = 0;
  }

  addScore(pts) {
    this.score += pts;
    this.scoreDisplay.innerText = this.score;

    if (this.score > this.bestScore) {
      this.bestScore = this.score;
      localStorage.setItem('AR_FRUIT_BEST', this.bestScore.toString());
    }

    // Check difficulty checkpoints (100 -> 180° -> 270° -> 360°)
    this.checkCheckpoints();
  }

  checkCheckpoints() {
    let newTier = 0;
    for (let i = this.tiers.length - 1; i >= 0; i--) {
      if (this.score >= this.tiers[i].minScore) {
        newTier = i;
        break;
      }
    }

    if (newTier !== this.currentTierIndex) {
      this.currentTierIndex = newTier;
      const tier = this.tiers[newTier];
      this.arcDisplay.innerText = `${tier.arc}° (${tier.name.split(':')[1].trim()})`;

      // Show checkpoint announcement banner
      this.checkpointSub.innerText = `${tier.arc}° ARC UNLOCKED!`;
      this.checkpointBanner.classList.remove('hidden');
      this.audio.playCheckpoint();
      setTimeout(() => this.checkpointBanner.classList.add('hidden'), 2200);
    }
  }

  missFruit() {
    this.lives--;
    this.audio.playMiss();

    let hearts = '';
    for (let i = 0; i < this.lives; i++) hearts += '❤️';
    if (this.lives <= 0) hearts = '💀';
    this.livesDisplay.innerText = hearts;

    if (this.lives <= 0) {
      this.triggerGameOver("Out of Lives!");
    }
  }

  triggerGameOver(reason) {
    this.isGameOver = true;
    this.gameOverReason.innerText = reason;
    this.finalScoreEl.innerText = this.score;
    this.bestScoreEl.innerText = this.bestScore;
    this.gameOverModal.classList.remove('hidden');
  }

  restartGame() {
    // Reset state
    this.score = 0;
    this.lives = 3;
    this.currentTierIndex = 0;
    this.isGameOver = false;

    this.scoreDisplay.innerText = '0';
    this.livesDisplay.innerText = '❤️❤️❤️';
    this.arcDisplay.innerText = '100° (Front)';
    this.gameOverModal.classList.add('hidden');
    this.rearAlert.classList.add('hidden');
    this.activeThreat = null;

    // Clear fruits
    this.fruits.forEach(f => this.scene.remove(f.mesh));
    this.fruits = [];
    this.halves.forEach(h => this.scene.remove(h.mesh));
    this.halves = [];
    this.particles.forEach(p => this.scene.remove(p.mesh));
    this.particles = [];

    this.startSpawnLoop();
  }

  // --- Animation & Physics Loop ---
  animate() {
    requestAnimationFrame(() => this.animate());

    const dt = 0.016; // approx 60fps
    const gravity = -9.8;

    // Update Camera Heading based on cameraYaw
    this.camera.rotation.y = this.cameraYaw;

    // Update Fruits
    for (let i = this.fruits.length - 1; i >= 0; i--) {
      const f = this.fruits[i];
      if (!f.isSliced) {
        f.velocity.y += gravity * dt;
        f.mesh.position.addScaledVector(f.velocity, dt);

        f.mesh.rotation.x += f.spin.x * dt;
        f.mesh.rotation.y += f.spin.y * dt;
        f.mesh.rotation.z += f.spin.z * dt;

        // If fell below ground level (-0.5m)
        if (f.mesh.position.y < -0.6) {
          this.scene.remove(f.mesh);
          this.fruits.splice(i, 1);
          if (!f.data.isBomb && !this.isGameOver) {
            this.missFruit();
          }
        }
      } else {
        this.fruits.splice(i, 1);
      }
    }

    // Update Halves
    const now = performance.now();
    for (let i = this.halves.length - 1; i >= 0; i--) {
      const h = this.halves[i];
      h.velocity.y += gravity * dt;
      h.mesh.position.addScaledVector(h.velocity, dt);

      h.mesh.rotation.x += h.spin.x * dt;
      h.mesh.rotation.y += h.spin.y * dt;

      if (now - h.spawnTime > 1800 || h.mesh.position.y < -2) {
        this.scene.remove(h.mesh);
        this.halves.splice(i, 1);
      }
    }

    // Update Particles
    for (let i = this.particles.length - 1; i >= 0; i--) {
      const p = this.particles[i];
      p.velocity.y += gravity * 1.5 * dt;
      p.mesh.position.addScaledVector(p.velocity, dt);

      if (now - p.spawnTime > 800) {
        this.scene.remove(p.mesh);
        this.particles.splice(i, 1);
      }
    }

    // Check combo timeout
    if (this.comboStreak > 0 && now - this.lastSliceTime > this.comboResetTime) {
      this.finishCombo();
    }

    // Update Rear threat alert
    this.updateRearAlert();

    // Render 3D Scene
    this.renderer.render(this.scene, this.camera);

    // Render 2D Neon Blade Trail
    this.renderBladeTrail(now);
  }

  renderBladeTrail(now) {
    this.bladeCtx.clearRect(0, 0, this.bladeCanvas.width, this.bladeCanvas.height);

    // Prune old blade points (> 180ms)
    this.bladePoints = this.bladePoints.filter(p => now - p.time < 180);

    if (this.bladePoints.length < 2) return;

    this.bladeCtx.lineCap = 'round';
    this.bladeCtx.lineJoin = 'round';

    for (let i = 1; i < this.bladePoints.length; i++) {
      const p1 = this.bladePoints[i - 1];
      const p2 = this.bladePoints[i];
      const ageRatio = (now - p2.time) / 180;
      const alpha = Math.max(0, 1 - ageRatio);
      const width = (1 - ageRatio) * 12 + 2;

      // Outer Cyan Glow
      this.bladeCtx.beginPath();
      this.bladeCtx.moveTo(p1.x, p1.y);
      this.bladeCtx.lineTo(p2.x, p2.y);
      this.bladeCtx.strokeStyle = `rgba(0, 242, 254, ${alpha * 0.8})`;
      this.bladeCtx.lineWidth = width * 1.8;
      this.bladeCtx.stroke();

      // Inner White Core
      this.bladeCtx.beginPath();
      this.bladeCtx.moveTo(p1.x, p1.y);
      this.bladeCtx.lineTo(p2.x, p2.y);
      this.bladeCtx.strokeStyle = `rgba(255, 255, 255, ${alpha})`;
      this.bladeCtx.lineWidth = width * 0.6;
      this.bladeCtx.stroke();
    }
  }
}

// Start Game on Page Load
window.addEventListener('DOMContentLoaded', () => {
  window.game = new ARFruitSlasherGame();
});
