<template>
  <PageLayout narrow>
    <section class="login-card">
      <div class="login-card__header">
        <span class="login-card__logo">O</span>
        <h1>Logowanie</h1>
        <p>Wybierz konto kliniki albo pacjenta, aby przejsc do odpowiedniego widoku.</p>
      </div>

      <AppAlert v-if="error" :message="error" variant="error" @dismiss="error = null" />

      <div class="login-presets">
        <button type="button" class="login-preset" @click="fillClinic">
          <strong>Klinika</strong>
          <span>klinika@example.com / klinika123</span>
        </button>
        <button type="button" class="login-preset" @click="fillPatient">
          <strong>Pacjent</strong>
          <span>pacjent@example.com / pacjent123</span>
        </button>
      </div>

      <form class="login-form" @submit.prevent="handleSubmit">
        <label class="login-field">
          <span>Email</span>
          <input v-model.trim="email" type="email" autocomplete="username" required />
        </label>

        <label class="login-field">
          <span>Haslo</span>
          <input v-model="password" type="password" autocomplete="current-password" required />
        </label>

        <button type="submit" class="app-btn app-btn--primary" :disabled="submitting">
          {{ submitting ? 'Logowanie...' : 'Zaloguj' }}
        </button>
      </form>
    </section>
  </PageLayout>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import PageLayout from '../../components/common/PageLayout.vue'
import AppAlert from '../../components/common/AppAlert.vue'
import { login } from '../../services/authService'
import { getApiErrorMessage } from '../../utils/apiError'

const router = useRouter()
const email = ref('klinika@example.com')
const password = ref('klinika123')
const submitting = ref(false)
const error = ref<string | null>(null)

const fillClinic = () => {
  email.value = 'klinika@example.com'
  password.value = 'klinika123'
}

const fillPatient = () => {
  email.value = 'pacjent@example.com'
  password.value = 'pacjent123'
}

const handleSubmit = async () => {
  submitting.value = true
  error.value = null

  try {
    const session = await login(email.value, password.value)
    if (session.mustChangePassword) {
      router.push({ name: 'change-password' })
      return
    }

    router.push({ name: session.role === 'Patient' ? 'patient-portal' : 'dashboard' })
  } catch (err) {
    error.value = getApiErrorMessage(err, 'Nie udalo sie zalogowac.')
  } finally {
    submitting.value = false
  }
}
</script>

<style scoped>
.login-card {
  margin: 3rem auto 0;
  padding: 2rem;
  background: var(--color-surface);
  border: 1px solid var(--color-border);
  border-radius: var(--radius-lg);
  box-shadow: var(--shadow-md);
}

.login-card__header {
  display: grid;
  justify-items: start;
  gap: 0.55rem;
  margin-bottom: 1.25rem;
}

.login-card__logo {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  width: 2.4rem;
  height: 2.4rem;
  border-radius: 999px;
  background: var(--color-primary);
  color: #fff;
  font-weight: 800;
}

.login-card h1 {
  margin: 0;
  font-size: 1.7rem;
}

.login-card p {
  margin: 0;
  color: var(--color-text-muted);
  line-height: 1.5;
}

.login-presets {
  display: grid;
  grid-template-columns: repeat(2, minmax(0, 1fr));
  gap: 0.75rem;
  margin-bottom: 1.25rem;
}

.login-preset {
  display: grid;
  gap: 0.25rem;
  padding: 0.85rem;
  border: 1px solid var(--color-border);
  border-radius: var(--radius-sm);
  background: #f8fafc;
  color: var(--color-text);
  cursor: pointer;
  font: inherit;
  text-align: left;
}

.login-preset:hover {
  border-color: var(--color-primary);
  background: #eff6ff;
}

.login-preset span {
  color: var(--color-text-muted);
  font-size: 0.78rem;
}

.login-form,
.login-field {
  display: grid;
  gap: 0.75rem;
}

.login-field span {
  color: var(--color-text-muted);
  font-size: 0.78rem;
  font-weight: 800;
  text-transform: uppercase;
}

.login-field input {
  width: 100%;
  min-height: 2.8rem;
  border: 1px solid var(--color-border-strong);
  border-radius: var(--radius-sm);
  color: var(--color-text);
  font: inherit;
  padding: 0.65rem 0.8rem;
}

.login-field input:focus {
  outline: none;
  border-color: var(--color-primary);
  box-shadow: 0 0 0 3px var(--color-primary-soft);
}

@media (max-width: 640px) {
  .login-card {
    margin-top: 1rem;
    padding: 1.25rem;
  }

  .login-presets {
    grid-template-columns: 1fr;
  }
}
</style>
