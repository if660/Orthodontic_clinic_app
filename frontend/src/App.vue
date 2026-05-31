<script setup lang="ts">
import { RouterLink, RouterView } from 'vue-router'
</script>

<template>
  <div class="app-shell">
    <nav class="app-navbar">
      <RouterLink to="/" class="app-navbar__brand">
        <span class="app-navbar__logo">O</span>
        <span>Orthodontic Clinic</span>
      </RouterLink>

      <div class="app-navbar__links">
        <RouterLink to="/" class="app-navbar__link">
          <span>⌂</span>
          <span>Panel główny</span>
        </RouterLink>

        <RouterLink to="/patients" class="app-navbar__link">
          <span>Pacjenci</span>
        </RouterLink>

        <RouterLink to="/doctors" class="app-navbar__link">
          <span>Lekarze</span>
        </RouterLink>
      </div>
    </nav>

    <RouterView />
  </div>
</template>

<style scoped>
.app-shell {
  min-height: 100vh;
}

.app-navbar {
  position: sticky;
  top: 0;
  z-index: 50;
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 1.5rem;
  padding: 0.875rem 1rem;
  background: rgba(255, 255, 255, 0.86);
  border-bottom: 1px solid var(--color-border);
  backdrop-filter: blur(14px);
  box-shadow: var(--shadow-sm);
}

.app-navbar__brand {
  display: inline-flex;
  align-items: center;
  gap: 0.65rem;
  color: var(--color-text);
  text-decoration: none;
  font-weight: 800;
  letter-spacing: -0.02em;
  white-space: nowrap;
}

.app-navbar__logo {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  width: 2rem;
  height: 2rem;
  border-radius: 999px;
  background: var(--color-primary);
  color: #fff;
  font-weight: 800;
  box-shadow: 0 3px 10px rgba(37, 99, 235, 0.28);
}

.app-navbar__links {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  flex-wrap: wrap;
  justify-content: flex-end;
}

.app-navbar__link {
  display: inline-flex;
  align-items: center;
  gap: 0.45rem;
  padding: 0.55rem 0.85rem;
  border-radius: var(--radius-sm);
  color: var(--color-text-muted);
  text-decoration: none;
  font-size: 0.9rem;
  font-weight: 650;
  transition:
    background-color var(--transition-fast),
    color var(--transition-fast),
    transform var(--transition-fast);
}

.app-navbar__link:hover {
  background: var(--color-primary-soft);
  color: var(--color-primary);
  transform: translateY(-1px);
}

.app-navbar__link.router-link-exact-active {
  background: var(--color-primary);
  color: #fff;
  box-shadow: 0 2px 8px rgba(37, 99, 235, 0.25);
}

@media (max-width: 700px) {
  .app-navbar {
    align-items: flex-start;
    flex-direction: column;
    gap: 0.75rem;
  }

  .app-navbar__links {
    width: 100%;
    justify-content: flex-start;
  }

  .app-navbar__link {
    padding: 0.5rem 0.7rem;
  }
}

@media (max-width: 430px) {
  .app-navbar__brand span:last-child {
    display: none;
  }

  .app-navbar__link span:last-child {
    font-size: 0.82rem;
  }
}
</style>