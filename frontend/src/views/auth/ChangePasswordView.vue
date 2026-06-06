<template>
  <PageLayout narrow>
    <section class="password-card">
      <div class="password-card__header">
        <span class="password-card__logo">O</span>
        <h1>Zmien haslo</h1>
        <p>To konto ma haslo startowe. Ustaw nowe haslo, zanim przejdziesz dalej.</p>
      </div>

      <AppAlert v-if="error" :message="error" variant="error" @dismiss="error = null" />

      <form class="password-form" @submit.prevent="handleSubmit">
        <label class="password-field">
          <span>Aktualne haslo</span>
          <input v-model="currentPassword" type="password" autocomplete="current-password" required />
        </label>

        <label class="password-field">
          <span>Nowe haslo</span>
          <input v-model="newPassword" type="password" autocomplete="new-password" required />
        </label>

        <label class="password-field">
          <span>Powtorz nowe haslo</span>
          <input v-model="confirmPassword" type="password" autocomplete="new-password" required />
        </label>

        <ul class="password-rules">
          <li :class="{ 'password-rules__item--ok': rules.length }">Minimum 8 znakow</li>
          <li :class="{ 'password-rules__item--ok': rules.lowercase }">Mala litera</li>
          <li :class="{ 'password-rules__item--ok': rules.uppercase }">Duza litera</li>
          <li :class="{ 'password-rules__item--ok': rules.digit }">Cyfra</li>
          <li :class="{ 'password-rules__item--ok': rules.special }">Znak specjalny</li>
        </ul>

        <button type="submit" class="app-btn app-btn--primary" :disabled="submitting || !isValid">
          {{ submitting ? 'Zapisywanie...' : 'Zmien haslo' }}
        </button>
      </form>
    </section>
  </PageLayout>
</template>

<script setup lang="ts">
import { computed, ref } from 'vue'
import { useRouter } from 'vue-router'
import PageLayout from '../../components/common/PageLayout.vue'
import AppAlert from '../../components/common/AppAlert.vue'
import { changePassword } from '../../services/authService'
import { getApiErrorMessage } from '../../utils/apiError'

const router = useRouter()
const currentPassword = ref('')
const newPassword = ref('')
const confirmPassword = ref('')
const submitting = ref(false)
const error = ref<string | null>(null)

const rules = computed(() => ({
  length: newPassword.value.length >= 8,
  lowercase: /[a-z]/.test(newPassword.value),
  uppercase: /[A-Z]/.test(newPassword.value),
  digit: /[0-9]/.test(newPassword.value),
  special: /[^a-zA-Z0-9]/.test(newPassword.value),
}))

const isValid = computed(() =>
  rules.value.length &&
  rules.value.lowercase &&
  rules.value.uppercase &&
  rules.value.digit &&
  rules.value.special &&
  newPassword.value === confirmPassword.value,
)

const handleSubmit = async () => {
  if (!isValid.value) {
    error.value = 'Haslo musi spelniac wszystkie wymagania i zgadzac sie z potwierdzeniem.'
    return
  }

  submitting.value = true
  error.value = null

  try {
    const session = await changePassword(currentPassword.value, newPassword.value, confirmPassword.value)
    router.push({ name: session.role === 'Patient' ? 'patient-portal' : 'dashboard' })
  } catch (err) {
    error.value = getApiErrorMessage(err, 'Nie udalo sie zmienic hasla.')
  } finally {
    submitting.value = false
  }
}
</script>

<style scoped>
.password-card {
  margin: 3rem auto 0;
  padding: 2rem;
  background: var(--color-surface);
  border: 1px solid var(--color-border);
  border-radius: var(--radius-lg);
  box-shadow: var(--shadow-md);
}

.password-card__header {
  display: grid;
  gap: 0.55rem;
  margin-bottom: 1.25rem;
}

.password-card__logo {
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

.password-card h1 {
  margin: 0;
  font-size: 1.7rem;
}

.password-card p {
  margin: 0;
  color: var(--color-text-muted);
  line-height: 1.5;
}

.password-form,
.password-field {
  display: grid;
  gap: 0.75rem;
}

.password-field span {
  color: var(--color-text-muted);
  font-size: 0.78rem;
  font-weight: 800;
  text-transform: uppercase;
}

.password-field input {
  width: 100%;
  min-height: 2.8rem;
  border: 1px solid var(--color-border-strong);
  border-radius: var(--radius-sm);
  color: var(--color-text);
  font: inherit;
  padding: 0.65rem 0.8rem;
}

.password-field input:focus {
  outline: none;
  border-color: var(--color-primary);
  box-shadow: 0 0 0 3px var(--color-primary-soft);
}

.password-rules {
  display: grid;
  gap: 0.35rem;
  margin: 0;
  padding-left: 1.2rem;
  color: var(--color-text-muted);
  font-size: 0.85rem;
}

.password-rules__item--ok {
  color: #15803d;
  font-weight: 700;
}
</style>
