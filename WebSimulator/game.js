// ============================================================================
// AR 3D FRUIT SLASHER - INTERACTIVE WEB SIMULATOR
// Features: AR Face Detection Scanner, Distinct 3D Fruit Types (Watermelon,
// Orange, Apple, Pineapple, Bomb), 360° Arc Scaling, Rear Alert Radar, Combos
// ============================================================================

// --- Procedural 2D Texture Generators for Realistic Fruit Skins & Slices ---
const FruitTextureGenerator = {
  createWatermelonRind() {
    const canvas = document.createElement('canvas');
    canvas.width = 512;
    canvas.height = 256;
    const ctx = canvas.getContext('2d');

    // Base light/medium green
    ctx.fillStyle = '#228b3b';
    ctx.fillRect(0, 0, 512, 256);

    // Dark wavy stripes
    ctx.fillStyle = '#0f481c';
    const stripeCount = 14;
    const stripeWidth = 512 / stripeCount;
    for (let s = 0; s < stripeCount; s++) {
      ctx.beginPath();
      const startX = s * stripeWidth;
      ctx.moveTo(startX, 0);
      for (let y = 0; y <= 256; y += 16) {
        const offset = Math.sin((y / 256) * Math.PI * 6 + s) * 12;
        ctx.lineTo(startX + offset, y);
      }
      for (let y = 256; y >= 0; y -= 16) {
        const offset = Math.sin((y / 256) * Math.PI * 6 + s) * 12;
        ctx.lineTo(startX + 18 + offset, y);
      }
      ctx.closePath();
      ctx.fill();
    }
    const tex = new THREE.CanvasTexture(canvas);
    tex.wrapS = THREE.RepeatWrapping;
    tex.wrapT = THREE.ClampToEdgeWrapping;
    return tex;
  },

  createWatermelonFlesh() {
    const canvas = document.createElement('canvas');
    canvas.width = 256;
    canvas.height = 256;
    const ctx = canvas.getContext('2d');

    // Dark green outer rind
    ctx.fillStyle = '#0f481c';
    ctx.beginPath();
    ctx.arc(128, 128, 126, 0, Math.PI * 2);
    ctx.fill();

    // Light green/white pith
    ctx.fillStyle = '#c7f0b8';
    ctx.beginPath();
    ctx.arc(128, 128, 118, 0, Math.PI * 2);
    ctx.fill();

    // Vibrant red center flesh
    ctx.fillStyle = '#e81e3a';
    ctx.beginPath();
    ctx.arc(128, 128, 108, 0, Math.PI * 2);
    ctx.fill();

    // Black watermelon seeds
    ctx.fillStyle = '#111';
    const seedCount = 12;
    for (let i = 0; i < seedCount; i++) {
      const angle = (i / seedCount) * Math.PI * 2;
      const r = 55 + (i % 2) * 20;
      const x = 128 + Math.cos(angle) * r;
      const y = 128 + Math.sin(angle) * r;
      ctx.beginPath();
      ctx.ellipse(x, y, 4, 7, angle + Math.PI / 2, 0, Math.PI * 2);
      ctx.fill();
    }
    return new THREE.CanvasTexture(canvas);
  },

  createOrangeRind() {
    const canvas = document.createElement('canvas');
    canvas.width = 256;
    canvas.height = 256;
    const ctx = canvas.getContext('2d');
    ctx.fillStyle = '#ff7b00';
    ctx.fillRect(0, 0, 256, 256);

    // Dimpled texture dots
    ctx.fillStyle = '#e66a00';
    for (let i = 0; i < 400; i++) {
      const x = Math.random() * 256;
      const y = Math.random() * 256;
      ctx.beginPath();
      ctx.arc(x, y, 1.5, 0, Math.PI * 2);
      ctx.fill();
    }
    return new THREE.CanvasTexture(canvas);
  },

  createOrangeFlesh() {
    const canvas = document.createElement('canvas');
    canvas.width = 256;
    canvas.height = 256;
    const ctx = canvas.getContext('2d');

    // Orange rind
    ctx.fillStyle = '#ff7b00';
    ctx.beginPath();
    ctx.arc(128, 128, 126, 0, Math.PI * 2);
    ctx.fill();

    // White pith
    ctx.fillStyle = '#fff4e6';
    ctx.beginPath();
    ctx.arc(128, 128, 116, 0, Math.PI * 2);
    ctx.fill();

    // Citrus segments (8 triangular spokes)
    const segments = 8;
    for (let s = 0; s < segments; s++) {
      const a1 = (s / segments) * Math.PI * 2 + 0.08;
      const a2 = ((s + 1) / segments) * Math.PI * 2 - 0.08;
      ctx.fillStyle = '#ffa200';
      ctx.beginPath();
      ctx.moveTo(128, 128);
      ctx.arc(128, 128, 108, a1, a2);
      ctx.closePath();
      ctx.fill();
    }

    // Center white dot
    ctx.fillStyle = '#fff4e6';
    ctx.beginPath();
    ctx.arc(128, 128, 12, 0, Math.PI * 2);
    ctx.fill();

    return new THREE.CanvasTexture(canvas);
  },

  createAppleFlesh() {
    const canvas = document.createElement('canvas');
    canvas.width = 256;
    canvas.height = 256;
    const ctx = canvas.getContext('2d');

    // Red skin rim
    ctx.fillStyle = '#cc1824';
    ctx.beginPath();
    ctx.arc(128, 128, 126, 0, Math.PI * 2);
    ctx.fill();

    // Creamy white flesh
    ctx.fillStyle = '#fcf8dc';
    ctx.beginPath();
    ctx.arc(128, 128, 120, 0, Math.PI * 2);
    ctx.fill();

    // Star core & 2 brown seeds
    ctx.fillStyle = '#d4c798';
    ctx.beginPath();
    ctx.arc(128, 128, 22, 0, Math.PI * 2);
    ctx.fill();

    ctx.fillStyle = '#5c2c16';
    ctx.beginPath();
    ctx.ellipse(122, 128, 3, 6, 0.3, 0, Math.PI * 2);
    ctx.ellipse(134, 128, 3, 6, -0.3, 0, Math.PI * 2);
    ctx.fill();

    return new THREE.CanvasTexture(canvas);
  },

  createPineappleSkin() {
    const canvas = document.createElement('canvas');
    canvas.width = 256;
    canvas.height = 256;
    const ctx = canvas.getContext('2d');
    ctx.fillStyle = '#d48817';
    ctx.fillRect(0, 0, 256, 256);

    // Crosshatch diamond pattern
    ctx.strokeStyle = '#804b08';
    ctx.lineWidth = 3;
    for (let i = -256; i < 512; i += 32) {
      ctx.beginPath();
      ctx.moveTo(i, 0);
      ctx.lineTo(i + 256, 256);
      ctx.stroke();

      ctx.beginPath();
      ctx.moveTo(i, 256);
      ctx.lineTo(i + 256, 0);
      ctx.stroke();
    }

    // Yellow diamond center spots
    ctx.fillStyle = '#ffd13b';
    for (let x = 16; x < 256; x += 32) {
      for (let y = 16; y < 256; y += 32) {
        ctx.beginPath();
        ctx.arc(x, y, 5, 0, Math.PI * 2);
        ctx.fill();
      }
    }
    return new THREE.CanvasTexture(canvas);
  }
};

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

  playBiometricLock() {
    if (!this.ctx) return;
    this.ensureContext();
    const now = this.ctx.currentTime;
    [880, 1174, 1760].forEach((freq, i) => {
      const osc = this.ctx.createOscillator();
      const gain = this.ctx.createGain();
      osc.type = 'sine';
      osc.frequency.setValueAtTime(freq, now + i * 0.08);
      gain.gain.setValueAtTime(0.3, now + i * 0.08);
      gain.gain.exponentialRampToValueAtTime(0.01, now + i * 0.08 + 0.18);
      osc.connect(gain);
      gain.connect(this.ctx.destination);
      osc.start(now + i * 0.08);
      osc.stop(now + i * 0.08 + 0.18);
    });
  }
}

// --- Main Game Implementation ---
class ARFruitSlasherGame {
  constructor() {
    this.audio = new SoundEngine();

    // Textures Cache
    this.textures = {
      watermelonRind: FruitTextureGenerator.createWatermelonRind(),
      watermelonFlesh: FruitTextureGenerator.createWatermelonFlesh(),
      orangeRind: FruitTextureGenerator.createOrangeRind(),
      orangeFlesh: FruitTextureGenerator.createOrangeFlesh(),
      appleFlesh: FruitTextureGenerator.createAppleFlesh(),
      pineappleSkin: FruitTextureGenerator.createPineappleSkin()
    };

    // Game States: 'FACE_SCAN', 'PLAYING', 'GAME_OVER'
    this.gameState = 'FACE_SCAN';

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
    this.cameraYaw = 0;
    this.isPanning = false;
    this.lastPanX = 0;

    // Blade Swipe
    this.isSwiping = false;
    this.bladePoints = [];
    this.previousScreenPoint = null;

    // Active Rear Threat
    this.activeThreat = null;

    // Face Scanner
    this.webcamStream = null;
    this.scanProgress = 0;
    this.isScanningFace = false;

    this.initThree();
    this.initBladeCanvas();
    this.initUI();
    this.initEvents();
    this.initFaceScanner();
    this.animate();
  }

  initThree() {
    this.threeCanvas = document.getElementById('three-canvas');
    this.renderer = new THREE.WebGLRenderer({ canvas: this.threeCanvas, antialias: true, alpha: true });
    this.renderer.setSize(window.innerWidth, window.innerHeight);
    this.renderer.setPixelRatio(Math.min(window.devicePixelRatio, 2));

    this.scene = new THREE.Scene();
    this.scene.background = new THREE.Color(0x0a0d14);

    // Camera at simulated eye level
    this.camera = new THREE.PerspectiveCamera(65, window.innerWidth / window.innerHeight, 0.1, 100);
    this.camera.position.set(0, 1.4, 0);

    // Ambient + Directional Lights
    const ambientLight = new THREE.AmbientLight(0xffffff, 0.85);
    this.scene.add(ambientLight);

    const sunLight = new THREE.DirectionalLight(0xfffaed, 1.4);
    sunLight.position.set(5, 10, 5);
    this.scene.add(sunLight);

    // Secondary fill light for vivid fruit colors
    const fillLight = new THREE.DirectionalLight(0x00f2fe, 0.4);
    fillLight.position.set(-5, 4, -5);
    this.scene.add(fillLight);

    // Futuristic AR Reticle Ground Floor
    const grid = new THREE.GridHelper(12, 24, 0x00f2fe, 0x1f293d);
    grid.position.y = -0.05;
    this.scene.add(grid);

    // Circular ground ring
    const ringGeo = new THREE.RingGeometry(2.4, 2.5, 48);
    const ringMat = new THREE.MeshBasicMaterial({ color: 0x00f2fe, side: THREE.DoubleSide, transparent: true, opacity: 0.3 });
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

    this.fruitTypeBadge = document.getElementById('fruit-type-badge');
    this.fruitBadgeIcon = document.getElementById('fruit-badge-icon');
    this.fruitBadgeName = document.getElementById('fruit-badge-name');

    // AR PIP Badge
    this.arCamPip = document.getElementById('ar-cam-pip');
    this.pipVideo = document.getElementById('pip-video');

    // Face Scan Modal Elements
    this.faceScanModal = document.getElementById('face-scan-modal');
    this.scannerVideo = document.getElementById('scanner-video');
    this.scanProgressBar = document.getElementById('scan-progress-bar');
    this.scanPercent = document.getElementById('scan-percent');
    this.scanStatusText = document.getElementById('scan-status-text');
    this.startCamBtn = document.getElementById('start-cam-btn');
    this.bypassScanBtn = document.getElementById('bypass-scan-btn');

    // Game Over Elements
    this.gameOverModal = document.getElementById('game-over-modal');
    this.gameOverReason = document.getElementById('game-over-reason');
    this.finalScoreEl = document.getElementById('final-score');
    this.bestScoreEl = document.getElementById('best-score');
    this.restartBtn = document.getElementById('restart-btn');
    this.rescanFaceBtn = document.getElementById('rescan-face-btn');

    this.restartBtn.addEventListener('click', () => this.restartGame());
    if (this.rescanFaceBtn) {
      this.rescanFaceBtn.addEventListener('click', () => {
        this.gameOverModal.classList.add('hidden');
        this.openFaceScanner();
      });
    }
  }

  initEvents() {
    window.addEventListener('resize', () => {
      this.camera.aspect = window.innerWidth / window.innerHeight;
      this.camera.updateProjectionMatrix();
      this.renderer.setSize(window.innerWidth, window.innerHeight);
      this.resizeBladeCanvas();
    });

    // Keyboard rotation
    window.addEventListener('keydown', (e) => {
      if (this.gameState !== 'PLAYING') return;
      if (e.key === 'a' || e.key === 'ArrowLeft') {
        this.cameraYaw += 0.15;
      } else if (e.key === 'd' || e.key === 'ArrowRight') {
        this.cameraYaw -= 0.15;
      }
    });

    // Mouse / Touch Controls
    window.addEventListener('mousedown', (e) => {
      if (this.gameState !== 'PLAYING') return;
      if (e.button === 0) {
        this.isSwiping = true;
        this.audio.playSwish();
        this.recordBladePoint(e.clientX, e.clientY);
      } else if (e.button === 2) {
        this.isPanning = true;
        this.lastPanX = e.clientX;
      }
    });

    window.addEventListener('mousemove', (e) => {
      if (this.gameState !== 'PLAYING') return;
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

    // Touch Controls
    window.addEventListener('touchstart', (e) => {
      if (this.gameState !== 'PLAYING') return;
      if (e.touches.length === 1) {
        this.isSwiping = true;
        this.audio.playSwish();
        const t = e.touches[0];
        this.recordBladePoint(t.clientX, t.clientY);
      }
    });

    window.addEventListener('touchmove', (e) => {
      if (this.gameState !== 'PLAYING') return;
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

    // Mobile Gyroscope
    if (window.DeviceOrientationEvent) {
      window.addEventListener('deviceorientation', (e) => {
        if (this.gameState === 'PLAYING' && e.alpha !== null) {
          this.cameraYaw = THREE.MathUtils.degToRad(-e.alpha);
        }
      });
    }
  }

  // --- AR Face Scanner Authentication Workflow ---
  initFaceScanner() {
    this.startCamBtn.addEventListener('click', () => this.requestWebcamAndScan());
    this.bypassScanBtn.addEventListener('click', () => this.runFaceScanSimulation(true));

    // Try auto-requesting webcam on load
    this.requestWebcamAndScan();
  }

  openFaceScanner() {
    this.gameState = 'FACE_SCAN';
    this.scanProgress = 0;
    this.scanProgressBar.style.width = '0%';
    this.scanPercent.innerText = '0% ANALYZED';
    this.faceScanModal.classList.remove('hidden');
    this.arCamPip.classList.add('hidden');
    this.scanStatusText.innerText = 'ALIGN FACE IN RETICLE...';
    this.requestWebcamAndScan();
  }

  async requestWebcamAndScan() {
    try {
      this.scanStatusText.innerText = 'ACCESSING CAMERA...';
      const stream = await navigator.mediaDevices.getUserMedia({
        video: { facingMode: 'user', width: { ideal: 640 }, height: { ideal: 480 } },
        audio: false
      });
      this.webcamStream = stream;
      this.scannerVideo.srcObject = stream;
      this.pipVideo.srcObject = stream;
      this.scannerVideo.play();
      this.pipVideo.play();

      this.scanStatusText.innerText = 'FACE DETECTED! SCANNING...';
      this.runFaceScanSimulation(false);
    } catch (err) {
      console.warn("Webcam access unavailable or denied:", err);
      this.scanStatusText.innerText = 'CAMERA UNAVAILABLE (CLICK SIMULATE)';
    }
  }

  runFaceScanSimulation(isBypass = false) {
    if (this.isScanningFace) return;
    this.isScanningFace = true;
    this.scanProgress = 0;

    const intervalTime = isBypass ? 18 : 28;
    const timer = setInterval(() => {
      this.scanProgress += 2;
      this.scanProgressBar.style.width = `${this.scanProgress}%`;
      this.scanPercent.innerText = `${this.scanProgress}% ANALYZED`;

      if (this.scanProgress === 40) {
        this.scanStatusText.innerText = 'DETECTING FACIAL LANDMARKS...';
      } else if (this.scanProgress === 74) {
        this.scanStatusText.innerText = 'VERIFYING AR TRACKING MESH...';
      }

      if (this.scanProgress >= 100) {
        clearInterval(timer);
        this.isScanningFace = false;
        this.onFaceScanCompleted();
      }
    }, intervalTime);
  }

  onFaceScanCompleted() {
    this.scanStatusText.innerText = '✅ BIOMETRICS CONFIRMED: AGENT VERIFIED!';
    this.audio.playBiometricLock();

    setTimeout(() => {
      // Fade out scanner modal
      this.faceScanModal.classList.add('hidden');
      this.gameState = 'PLAYING';

      // Show in-game PIP AR face camera if stream is active
      if (this.webcamStream) {
        this.arCamPip.classList.remove('hidden');
      }

      // Show checkpoint / welcome banner
      this.checkpointSub.innerText = 'AR ARENA UNLOCKED - GET READY!';
      this.checkpointBanner.classList.remove('hidden');
      setTimeout(() => this.checkpointBanner.classList.add('hidden'), 2000);

      // Start fruit spawning
      this.startSpawnLoop();
    }, 900);
  }

  recordBladePoint(x, y) {
    this.bladePoints.push({ x, y, time: performance.now() });
    if (!this.previousScreenPoint) {
      this.previousScreenPoint = { x, y };
    }
  }

  // --- 3D Procedural Fruit Mesh Generation by Fruit Type ---
  createFruitByType(typeConfig) {
    const group = new THREE.Group();
    let mainMesh;

    switch (typeConfig.type) {
      case 'Watermelon': {
        // Oval watermelon geometry with procedural wavy green rind
        const geo = new THREE.SphereGeometry(0.34, 24, 24);
        geo.scale(1.2, 0.95, 0.95);
        const mat = new THREE.MeshStandardMaterial({
          map: this.textures.watermelonRind,
          roughness: 0.35,
          metalness: 0.05
        });
        mainMesh = new THREE.Mesh(geo, mat);
        group.add(mainMesh);
        break;
      }

      case 'Orange': {
        // Dimpled orange sphere with green stem leaf on top
        const geo = new THREE.SphereGeometry(0.26, 20, 20);
        const mat = new THREE.MeshStandardMaterial({
          map: this.textures.orangeRind,
          roughness: 0.45
        });
        mainMesh = new THREE.Mesh(geo, mat);
        group.add(mainMesh);

        // Green leaf on top
        const leafGeo = new THREE.ConeGeometry(0.04, 0.12, 5);
        const leafMat = new THREE.MeshBasicMaterial({ color: 0x228b3b });
        const leaf = new THREE.Mesh(leafGeo, leafMat);
        leaf.position.set(0.04, 0.26, 0);
        leaf.rotation.z = -0.5;
        group.add(leaf);
        break;
      }

      case 'Apple': {
        // Apple shape (slightly tapered bottom) + brown curved stem
        const geo = new THREE.SphereGeometry(0.23, 20, 20);
        geo.scale(1.0, 1.08, 1.0);
        const mat = new THREE.MeshStandardMaterial({
          color: 0xdd182b,
          roughness: 0.25,
          metalness: 0.1
        });
        mainMesh = new THREE.Mesh(geo, mat);
        group.add(mainMesh);

        // Brown stem
        const stemGeo = new THREE.CylinderGeometry(0.015, 0.015, 0.09, 6);
        const stemMat = new THREE.MeshBasicMaterial({ color: 0x4a2511 });
        const stem = new THREE.Mesh(stemGeo, stemMat);
        stem.position.set(0, 0.24, 0);
        stem.rotation.z = 0.2;
        group.add(stem);
        break;
      }

      case 'Pineapple': {
        // Golden pineapple cylinder body + crown of green spiky leaves
        const bodyGeo = new THREE.CylinderGeometry(0.20, 0.22, 0.38, 16);
        const bodyMat = new THREE.MeshStandardMaterial({
          map: this.textures.pineappleSkin,
          roughness: 0.5
        });
        mainMesh = new THREE.Mesh(bodyGeo, bodyMat);
        group.add(mainMesh);

        // Top green leaf crown (5 spiky leaves)
        for (let l = 0; l < 6; l++) {
          const leafGeo = new THREE.ConeGeometry(0.035, 0.22, 4);
          const leafMat = new THREE.MeshBasicMaterial({ color: 0x2e8b57 });
          const leaf = new THREE.Mesh(leafGeo, leafMat);
          const angle = (l / 6) * Math.PI * 2;
          leaf.position.set(Math.cos(angle) * 0.06, 0.24, Math.sin(angle) * 0.06);
          leaf.rotation.x = Math.sin(angle) * 0.5;
          leaf.rotation.z = -Math.cos(angle) * 0.5;
          group.add(leaf);
        }
        break;
      }

      case 'Bomb': {
        // Heavy metallic charcoal sphere + fuse cap + burning fuse spark
        const geo = new THREE.SphereGeometry(0.25, 18, 18);
        const mat = new THREE.MeshStandardMaterial({
          color: 0x18181c,
          metalness: 0.7,
          roughness: 0.3
        });
        mainMesh = new THREE.Mesh(geo, mat);
        group.add(mainMesh);

        // Brass fuse cap
        const capGeo = new THREE.CylinderGeometry(0.05, 0.05, 0.06, 8);
        const capMat = new THREE.MeshStandardMaterial({ color: 0xb59410, metalness: 0.8 });
        const cap = new THREE.Mesh(capGeo, capMat);
        cap.position.y = 0.25;
        group.add(cap);

        // Curved fuse cord
        const fuseGeo = new THREE.CylinderGeometry(0.015, 0.015, 0.12, 6);
        const fuseMat = new THREE.MeshBasicMaterial({ color: 0xddaa55 });
        const fuse = new THREE.Mesh(fuseGeo, fuseMat);
        fuse.position.set(0.03, 0.30, 0);
        fuse.rotation.z = -0.4;
        group.add(fuse);

        // Sparkling animated flame dot
        const sparkGeo = new THREE.SphereGeometry(0.035, 6, 6);
        const sparkMat = new THREE.MeshBasicMaterial({ color: 0xff4500 });
        const spark = new THREE.Mesh(sparkGeo, sparkMat);
        spark.position.set(0.06, 0.36, 0);
        group.add(spark);
        break;
      }
    }

    return { group, colliderMesh: mainMesh || group.children[0] };
  }

  // --- Spawner with Dynamic Angle Scaling ---
  startSpawnLoop() {
    const loop = () => {
      if (this.gameState === 'PLAYING' && !this.isGameOver) {
        const currentTier = this.tiers[this.currentTierIndex];
        this.spawnFruit(currentTier);

        if (currentTier.arc >= 270 && Math.random() < 0.35) {
          setTimeout(() => {
            if (this.gameState === 'PLAYING' && !this.isGameOver) {
              this.spawnFruit(currentTier);
            }
          }, 200);
        }

        setTimeout(loop, currentTier.interval * 1000);
      }
    };
    setTimeout(loop, 800);
  }

  spawnFruit(tier) {
    const halfArc = (tier.arc * 0.5) * (Math.PI / 180);
    const spawnAngleRel = (Math.random() * 2 - 1) * halfArc;
    const globalAngle = this.cameraYaw + spawnAngleRel;

    const radius = 2.4 + Math.random() * 0.8;
    const spawnX = Math.sin(globalAngle) * radius;
    const spawnZ = -Math.cos(globalAngle) * radius;
    const spawnY = -0.1; // Ground level

    const angleFromForward = Math.abs(spawnAngleRel) * (180 / Math.PI);
    const isBehind = angleFromForward > 55;

    // Check if bomb
    const isBomb = Math.random() < tier.bombChance;

    let typeConfig;
    if (isBomb) {
      typeConfig = {
        type: 'Bomb',
        name: 'BOMB HAZARD',
        icon: '💣',
        points: 0,
        isBomb: true,
        juiceColor: 0xff3300
      };
    } else {
      const catalog = [
        { type: 'Watermelon', name: 'WATERMELON', icon: '🍉', points: 30, isBomb: false, juiceColor: 0xe81e3a, fleshTexture: this.textures.watermelonFlesh },
        { type: 'Orange',     name: 'CITRUS ORANGE', icon: '🍊', points: 15, isBomb: false, juiceColor: 0xff9e00, fleshTexture: this.textures.orangeFlesh },
        { type: 'Apple',      name: 'CRISP APPLE',  icon: '🍎', points: 20, isBomb: false, juiceColor: 0xd90429, fleshTexture: this.textures.appleFlesh },
        { type: 'Pineapple',  name: 'GOLDEN PINEAPPLE', icon: '🍍', points: 50, isBomb: false, juiceColor: 0xffd13b, fleshTexture: this.textures.pineappleSkin }
      ];
      typeConfig = catalog[Math.floor(Math.random() * catalog.length)];
    }

    // Announce fruit type on HUD badge
    this.showFruitTypeBadge(typeConfig.icon, typeConfig.name);

    // Create 3D Fruit Model
    const fruitModel = this.createFruitByType(typeConfig);
    const group = fruitModel.group;
    group.position.set(spawnX, spawnY, spawnZ);

    // Launch velocity
    const upwardSpeed = 4.8 + Math.random() * 0.6;
    const inwardDir = new THREE.Vector3(-spawnX, 0, -spawnZ).normalize().multiplyScalar(1.0);
    const velocity = new THREE.Vector3(inwardDir.x, upwardSpeed, inwardDir.z);

    const spin = new THREE.Vector3(
      (Math.random() - 0.5) * 5,
      (Math.random() - 0.5) * 5,
      (Math.random() - 0.5) * 5
    );

    this.scene.add(group);
    this.fruits.push({
      group,
      colliderMesh: fruitModel.colliderMesh,
      velocity,
      spin,
      data: typeConfig,
      isSliced: false
    });

    // Trigger rear threat alert if outside front view
    if (isBehind) {
      this.triggerRearAlert(new THREE.Vector3(spawnX, spawnY, spawnZ));
    }
  }

  showFruitTypeBadge(icon, name) {
    this.fruitBadgeIcon.innerText = icon;
    this.fruitBadgeName.innerText = name;
    this.fruitTypeBadge.classList.remove('hidden');
    clearTimeout(this.badgeTimer);
    this.badgeTimer = setTimeout(() => {
      this.fruitTypeBadge.classList.add('hidden');
    }, 1800);
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

    const camDir = new THREE.Vector3(0, 0, -1).applyAxisAngle(new THREE.Vector3(0, 1, 0), this.cameraYaw);
    const toThreat = new THREE.Vector3(this.activeThreat.pos.x, 0, this.activeThreat.pos.z).normalize();

    const dot = camDir.dot(toThreat);
    const angleDeg = Math.acos(Math.max(-1, Math.min(1, dot))) * (180 / Math.PI);

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
      const targetMeshes = this.fruits.filter(f => !f.isSliced).map(f => f.colliderMesh);
      const intersects = this.raycaster.intersectObjects(targetMeshes);

      if (intersects.length > 0) {
        const hitMesh = intersects[0].object;
        const fruitObj = this.fruits.find(f => f.colliderMesh === hitMesh);
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
    this.scene.remove(fruitObj.group);

    if (fruitObj.data.isBomb) {
      this.audio.playExplosion();
      this.spawnBombExplosion(contactPoint);
      this.triggerGameOver("BOOM! You sliced a Bomb!");
      return;
    }

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

    // Add score
    this.addScore(fruitObj.data.points);

    // Spawn 2 split halves with fruit-specific flesh cross-sections
    this.spawnFruitHalves(fruitObj, contactPoint, swipeDir);

    // Spawn colored juice splatter
    this.spawnJuiceSplatter(contactPoint, fruitObj.data.juiceColor);

    // Floating 3D score popup
    this.spawnScorePopup(contactPoint, `+${fruitObj.data.points}`);
  }

  spawnFruitHalves(fruitObj, pos, swipeDir) {
    const halfGeo = new THREE.SphereGeometry(0.28, 16, 16, 0, Math.PI);
    const fleshTex = fruitObj.data.fleshTexture || this.textures.watermelonFlesh;

    const materials = [
      new THREE.MeshStandardMaterial({ map: fleshTex, roughness: 0.6 }),
      new THREE.MeshStandardMaterial({ color: fruitObj.data.juiceColor, roughness: 0.4 })
    ];

    const pushX = swipeDir.y;
    const pushZ = -swipeDir.x;

    [-1, 1].forEach((sign) => {
      const halfMesh = new THREE.Mesh(halfGeo, materials[0]);
      halfMesh.position.copy(pos);
      halfMesh.rotation.y = sign > 0 ? 0 : Math.PI;

      const vel = fruitObj.velocity.clone().multiplyScalar(0.7);
      vel.x += pushX * sign * 2.0;
      vel.z += pushZ * sign * 2.0;
      vel.y += 1.3;

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
    const count = 30;
    const pGeo = new THREE.SphereGeometry(0.045, 6, 6);
    const pMat = new THREE.MeshBasicMaterial({ color: colorHex });

    for (let i = 0; i < count; i++) {
      const p = new THREE.Mesh(pGeo, pMat);
      p.position.copy(pos);

      const vel = new THREE.Vector3(
        (Math.random() - 0.5) * 4.8,
        Math.random() * 3.8 + 0.6,
        (Math.random() - 0.5) * 4.8
      );

      this.scene.add(p);
      this.particles.push({ mesh: p, velocity: vel, spawnTime: performance.now() });
    }
  }

  spawnBombExplosion(pos) {
    const count = 55;
    const pGeo = new THREE.SphereGeometry(0.09, 6, 6);
    const pMat = new THREE.MeshBasicMaterial({ color: 0xff3300 });

    for (let i = 0; i < count; i++) {
      const p = new THREE.Mesh(pGeo, pMat);
      p.position.copy(pos);

      const vel = new THREE.Vector3(
        (Math.random() - 0.5) * 9.0,
        Math.random() * 6.5,
        (Math.random() - 0.5) * 9.0
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
    this.gameState = 'GAME_OVER';
    this.gameOverReason.innerText = reason;
    this.finalScoreEl.innerText = this.score;
    this.bestScoreEl.innerText = this.bestScore;
    this.gameOverModal.classList.remove('hidden');
  }

  restartGame() {
    this.score = 0;
    this.lives = 3;
    this.currentTierIndex = 0;
    this.isGameOver = false;
    this.gameState = 'PLAYING';

    this.scoreDisplay.innerText = '0';
    this.livesDisplay.innerText = '❤️❤️❤️';
    this.arcDisplay.innerText = '100° (Front)';
    this.gameOverModal.classList.add('hidden');
    this.rearAlert.classList.add('hidden');
    this.activeThreat = null;

    // Clear active fruits
    this.fruits.forEach(f => this.scene.remove(f.group));
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

    const dt = 0.016;
    const gravity = -9.8;

    this.camera.rotation.y = this.cameraYaw;

    // Update Fruits
    for (let i = this.fruits.length - 1; i >= 0; i--) {
      const f = this.fruits[i];
      if (!f.isSliced) {
        f.velocity.y += gravity * dt;
        f.group.position.addScaledVector(f.velocity, dt);

        f.group.rotation.x += f.spin.x * dt;
        f.group.rotation.y += f.spin.y * dt;
        f.group.rotation.z += f.spin.z * dt;

        if (f.group.position.y < -0.6) {
          this.scene.remove(f.group);
          this.fruits.splice(i, 1);
          if (!f.data.isBomb && !this.isGameOver && this.gameState === 'PLAYING') {
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

    // Render Blade Trail
    this.renderBladeTrail(now);
  }

  renderBladeTrail(now) {
    this.bladeCtx.clearRect(0, 0, this.bladeCanvas.width, this.bladeCanvas.height);

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

      // Cyan outer glow
      this.bladeCtx.beginPath();
      this.bladeCtx.moveTo(p1.x, p1.y);
      this.bladeCtx.lineTo(p2.x, p2.y);
      this.bladeCtx.strokeStyle = `rgba(0, 242, 254, ${alpha * 0.8})`;
      this.bladeCtx.lineWidth = width * 1.8;
      this.bladeCtx.stroke();

      // White inner core
      this.bladeCtx.beginPath();
      this.bladeCtx.moveTo(p1.x, p1.y);
      this.bladeCtx.lineTo(p2.x, p2.y);
      this.bladeCtx.strokeStyle = `rgba(255, 255, 255, ${alpha})`;
      this.bladeCtx.lineWidth = width * 0.6;
      this.bladeCtx.stroke();
    }
  }
}

// Start Game on Load
window.addEventListener('DOMContentLoaded', () => {
  window.game = new ARFruitSlasherGame();
});
