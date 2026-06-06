<script setup lang="ts">
import { computed, onMounted, onUnmounted } from 'vue'
import { RouterLink, RouterView, useRouter } from 'vue-router'
import { useIntervalFn, useNow } from '@vueuse/core'
import { useClinicStatsStore } from './stores/clinicStats'
import { authSession, isClinic, isPatient, logout } from './services/authService'

const statsStore = useClinicStatsStore()
const router = useRouter()
const now = useNow({ interval: 1000 })

const formattedTime = computed(() => {
  return new Intl.DateTimeFormat('pl-PL', {
    weekday: 'short',
    hour: '2-digit',
    minute: '2-digit',
    second: '2-digit',
  }).format(now.value)
})

const { pause, resume } = useIntervalFn(() => {
  statsStore.fetchStats()
}, 45000, { immediate: false })

onMounted(async () => {
  await statsStore.fetchStats()
  resume()
})

onUnmounted(() => {
  pause()
})

const handleLogout = () => {
  logout()
  router.push({ name: 'login' })
}
</script>

<template>
  <div class="app-shell">
    <nav class="app-navbar">
      <div class="app-navbar__left">
        <RouterLink to="/" class="app-navbar__brand">
          <span class="app-navbar__logo">O</span>
          <span>Orthodontic Clinic</span>
        </RouterLink>

        <div class="app-time-chip">
          <span class="pulse-dot" aria-hidden="true" />
          <span>{{ formattedTime }}</span>
        </div>
      </div>

      <div v-if="authSession" class="app-navbar__links">
        <RouterLink v-if="isClinic" to="/" class="app-navbar__link">
          <span>⌂</span>
          <span>Panel główny</span>
        </RouterLink>

        <RouterLink v-if="isClinic" to="/patients" class="app-navbar__link">
          <span>Pacjenci</span>
          <span class="app-navbar__badge">{{ statsStore.patientsCount }}</span>
        </RouterLink>

        <RouterLink v-if="isClinic" to="/doctors" class="app-navbar__link">
          <span>Lekarze</span>
          <span class="app-navbar__badge">{{ statsStore.doctorsCount }}</span>
        </RouterLink>

        <RouterLink v-if="isClinic" to="/appointments" class="app-navbar__link">
          <span>Kalendarz</span>
        </RouterLink>

        <RouterLink v-if="isClinic" to="/reports" class="app-navbar__link">
          <span>Raporty</span>
        </RouterLink>

        <RouterLink v-if="isPatient" to="/patient" class="app-navbar__link">
          <span>Moje wizyty</span>
        </RouterLink>

        <span class="app-navbar__user">
          {{ authSession.role === 'Clinic' ? 'Klinika' : authSession.patientName || 'Pacjent' }}
        </span>

        <button type="button" class="app-navbar__logout" @click="handleLogout">
          Wyloguj
        </button>
      </div>
    </nav>

    <RouterView v-slot="{ Component, route }">
      <Transition name="page-swoop" mode="out-in">
        <component :is="Component" :key="route.fullPath" />
      </Transition>
    </RouterView>
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

.app-navbar__left {
  display: flex;
  align-items: center;
  gap: 0.75rem;
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

.app-navbar__badge {
  min-width: 1.35rem;
  height: 1.35rem;
  border-radius: 999px;
  display: inline-flex;
  align-items: center;
  justify-content: center;
  background: rgba(255, 255, 255, 0.24);
  color: currentColor;
  font-size: 0.75rem;
  font-weight: 700;
}

.app-time-chip {
  display: inline-flex;
  align-items: center;
  gap: 0.45rem;
  border: 1px solid var(--color-border);
  background: #f8fafc;
  color: var(--color-text-muted);
  border-radius: 999px;
  padding: 0.4rem 0.65rem;
  font-size: 0.78rem;
  font-weight: 600;
}

.pulse-dot {
  width: 0.55rem;
  height: 0.55rem;
  border-radius: 999px;
  background: #0ea5e9;
  box-shadow: 0 0 0 rgba(14, 165, 233, 0.45);
  animation: pulse 1.8s infinite;
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

.app-navbar__user {
  color: var(--color-text-muted);
  font-size: 0.82rem;
  font-weight: 700;
  white-space: nowrap;
}

.app-navbar__logout {
  border: 1px solid var(--color-border-strong);
  border-radius: var(--radius-sm);
  background: #fff;
  color: var(--color-text);
  cursor: pointer;
  font: inherit;
  font-size: 0.84rem;
  font-weight: 700;
  padding: 0.5rem 0.75rem;
}

.app-navbar__logout:hover {
  border-color: var(--color-primary);
  color: var(--color-primary);
}

.page-swoop-enter-active,
.page-swoop-leave-active {
  transition:
    opacity 260ms ease,
    transform 260ms ease;
}

.page-swoop-enter-from,
.page-swoop-leave-to {
  opacity: 0;
  transform: translateY(10px) scale(0.995);
}

@keyframes pulse {
  0% {
    box-shadow: 0 0 0 0 rgba(14, 165, 233, 0.45);
  }

  70% {
    box-shadow: 0 0 0 8px rgba(14, 165, 233, 0);
  }

  100% {
    box-shadow: 0 0 0 0 rgba(14, 165, 233, 0);
  }
}

@media (max-width: 700px) {
  .app-navbar {
    align-items: flex-start;
    flex-direction: column;
    gap: 0.75rem;
  }

  .app-navbar__left {
    width: 100%;
    justify-content: space-between;
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

  .app-time-chip {
    font-size: 0.72rem;
    padding: 0.35rem 0.55rem;
  }

  .app-navbar__link span:last-child {
    font-size: 0.82rem;
  }
}
</style>
