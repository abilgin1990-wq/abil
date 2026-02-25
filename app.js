const canvas = document.getElementById("gameCanvas");
const ctx = canvas.getContext("2d");

const scoreEl = document.getElementById("score");
const highScoreEl = document.getElementById("highScore");
const speedEl = document.getElementById("speed");
const messageEl = document.getElementById("message");
const startBtn = document.getElementById("startBtn");
const restartBtn = document.getElementById("restartBtn");

const road = {
  x: 70,
  width: 280,
  lineHeight: 42,
  lineGap: 26,
  lineOffset: 0,
};

const player = {
  width: 42,
  height: 76,
  x: canvas.width / 2 - 21,
  y: canvas.height - 96,
  speed: 5,
};

const state = {
  running: false,
  score: 0,
  highScore: Number(localStorage.getItem("racingHighScore")) || 0,
  gameSpeed: 3,
  enemies: [],
  keys: {
    ArrowLeft: false,
    ArrowRight: false,
    ArrowUp: false,
    ArrowDown: false,
  },
  enemySpawnTimer: 0,
  lastTimestamp: 0,
};

highScoreEl.textContent = state.highScore;

function createEnemy() {
  const width = 42;
  const height = 76;
  const minX = road.x + 12;
  const maxX = road.x + road.width - width - 12;

  return {
    width,
    height,
    x: Math.random() * (maxX - minX) + minX,
    y: -height - 20,
    speed: state.gameSpeed + Math.random() * 1.4,
  };
}

function resetGame() {
  state.score = 0;
  state.gameSpeed = 3;
  state.enemies = [];
  state.enemySpawnTimer = 0;
  state.lastTimestamp = 0;
  player.x = canvas.width / 2 - player.width / 2;
  scoreEl.textContent = "0";
  speedEl.textContent = "1";
  messageEl.textContent = "Bol şans!";
  messageEl.classList.remove("over");
}

function drawCar({ x, y, width, height }, color) {
  ctx.fillStyle = color;
  ctx.fillRect(x, y, width, height);

  ctx.fillStyle = "#111827";
  ctx.fillRect(x + 6, y + 12, width - 12, 20);
  ctx.fillRect(x + 7, y + height - 18, width - 14, 10);

  ctx.fillStyle = "#f1f5f9";
  ctx.fillRect(x + 5, y + 2, 8, 8);
  ctx.fillRect(x + width - 13, y + 2, 8, 8);
  ctx.fillRect(x + 5, y + height - 10, 8, 8);
  ctx.fillRect(x + width - 13, y + height - 10, 8, 8);
}

function drawRoad(deltaFactor) {
  ctx.fillStyle = "#0f172a";
  ctx.fillRect(0, 0, canvas.width, canvas.height);

  ctx.fillStyle = "#475569";
  ctx.fillRect(road.x, 0, road.width, canvas.height);

  road.lineOffset += (state.gameSpeed * 4 + 3) * deltaFactor;
  const repeatSize = road.lineHeight + road.lineGap;
  if (road.lineOffset > repeatSize) {
    road.lineOffset = 0;
  }

  ctx.fillStyle = "#f8fafc";

  for (let y = -repeatSize; y < canvas.height + repeatSize; y += repeatSize) {
    const drawY = y + road.lineOffset;
    ctx.fillRect(canvas.width / 2 - 5, drawY, 10, road.lineHeight);
  }

  ctx.fillStyle = "#64748b";
  ctx.fillRect(road.x, 0, 4, canvas.height);
  ctx.fillRect(road.x + road.width - 4, 0, 4, canvas.height);
}

function checkCollision(a, b) {
  return (
    a.x < b.x + b.width &&
    a.x + a.width > b.x &&
    a.y < b.y + b.height &&
    a.y + a.height > b.y
  );
}

function endGame() {
  state.running = false;
  messageEl.textContent = "Kaza yaptın! Yeniden başlatıp tekrar dene.";
  messageEl.classList.add("over");
  startBtn.disabled = false;
  restartBtn.disabled = false;

  if (state.score > state.highScore) {
    state.highScore = Math.floor(state.score);
    localStorage.setItem("racingHighScore", String(state.highScore));
    highScoreEl.textContent = state.highScore;
  }
}

function updatePlayer(deltaFactor) {
  let horizontalSpeed = player.speed;
  if (state.keys.ArrowUp) horizontalSpeed += 1.5;
  if (state.keys.ArrowDown) horizontalSpeed -= 1.2;

  if (state.keys.ArrowLeft) {
    player.x -= horizontalSpeed * deltaFactor;
  }
  if (state.keys.ArrowRight) {
    player.x += horizontalSpeed * deltaFactor;
  }

  const minX = road.x + 8;
  const maxX = road.x + road.width - player.width - 8;
  player.x = Math.max(minX, Math.min(maxX, player.x));
}

function updateEnemies(deltaFactor) {
  state.enemySpawnTimer += deltaFactor;

  const spawnInterval = Math.max(20, 68 - state.gameSpeed * 6);
  if (state.enemySpawnTimer >= spawnInterval) {
    state.enemySpawnTimer = 0;
    state.enemies.push(createEnemy());
  }

  state.enemies.forEach((enemy) => {
    enemy.y += enemy.speed * deltaFactor;
  });

  state.enemies = state.enemies.filter((enemy) => enemy.y < canvas.height + enemy.height);
}

function updateScore(deltaFactor) {
  state.score += 0.2 * state.gameSpeed * deltaFactor;

  if (Math.floor(state.score) > 0 && Math.floor(state.score) % 100 === 0) {
    state.gameSpeed = Math.min(8, state.gameSpeed + 0.003 * deltaFactor);
  } else {
    state.gameSpeed = Math.min(8, state.gameSpeed + 0.0015 * deltaFactor);
  }

  scoreEl.textContent = Math.floor(state.score);
  speedEl.textContent = (state.gameSpeed / 3).toFixed(1);
}

function render() {
  drawCar(player, "#06b6d4");
  state.enemies.forEach((enemy) => drawCar(enemy, "#f97316"));
}

function loop(timestamp) {
  if (!state.running) {
    drawRoad(1);
    render();
    return;
  }

  if (!state.lastTimestamp) {
    state.lastTimestamp = timestamp;
  }

  const delta = timestamp - state.lastTimestamp;
  state.lastTimestamp = timestamp;
  const deltaFactor = Math.min(2.5, delta / 16.67);

  drawRoad(deltaFactor);
  updatePlayer(deltaFactor);
  updateEnemies(deltaFactor);
  updateScore(deltaFactor);

  for (const enemy of state.enemies) {
    if (checkCollision(player, enemy)) {
      render();
      endGame();
      return;
    }
  }

  render();
  requestAnimationFrame(loop);
}

function startGame() {
  if (state.running) return;
  resetGame();
  state.running = true;
  startBtn.disabled = true;
  restartBtn.disabled = false;
  requestAnimationFrame(loop);
}

function restartGame() {
  resetGame();
  state.running = true;
  startBtn.disabled = true;
  restartBtn.disabled = false;
  requestAnimationFrame(loop);
}

document.addEventListener("keydown", (e) => {
  if (e.key in state.keys) {
    state.keys[e.key] = true;
    e.preventDefault();
  }
});

document.addEventListener("keyup", (e) => {
  if (e.key in state.keys) {
    state.keys[e.key] = false;
    e.preventDefault();
  }
});

startBtn.addEventListener("click", startGame);
restartBtn.addEventListener("click", restartGame);

drawRoad(1);
render();
