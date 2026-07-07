<script setup lang="ts">
import { ref } from "vue";
import { useRouter } from "vue-router";
import { api } from "../api";

const router = useRouter();

const email = ref("");
const password = ref("");
const confirmPassword = ref("");
const showPassword = ref(false);
const showConfirmPassword = ref(false);
const agreeToTerms = ref(false);
const isLoading = ref(false);
const errorMsg = ref("");
const validationErrors = ref<string[]>([]);
const successMsg = ref("");

const passwordStrength = ref(0);

function calculatePasswordStrength(pwd: string): number {
  let strength = 0;
  if (pwd.length >= 8) strength += 25;
  if (pwd.length >= 12) strength += 25;
  if (/[a-z]/.test(pwd) && /[A-Z]/.test(pwd)) strength += 25;
  if (/[0-9]/.test(pwd)) strength += 25;
  return Math.min(strength, 100);
}

function handlePasswordInput(): void {
  passwordStrength.value = calculatePasswordStrength(password.value);
}

function getPasswordStrengthColor(): string {
  if (passwordStrength.value < 33) return "#ef4444";
  if (passwordStrength.value < 66) return "#eab308";
  return "#22c55e";
}

async function handleRegister(): Promise<void> {
  errorMsg.value = "";
  validationErrors.value = [];
  successMsg.value = "";

  if (!email.value || !password.value || !confirmPassword.value) {
    errorMsg.value = "Please fill in all fields";
    return;
  }

  if (!agreeToTerms.value) {
    errorMsg.value = "You must agree to the terms and conditions";
    return;
  }

  if (password.value !== confirmPassword.value) {
    errorMsg.value = "Passwords do not match";
    return;
  }

  isLoading.value = true;

  try {
    const result = await api.auth.register(email.value, password.value);
    
    if (result.success) {
      successMsg.value = result.message || "Registration successful! Check your email to verify your account.";
      email.value = "";
      password.value = "";
      confirmPassword.value = "";
      
      setTimeout(() => {
        router.push({ name: "VerifyEmail" });
      }, 2000);
    } else {
      if (result.errorCode === "DUPLICATE_EMAIL") {
        errorMsg.value = "This email is already registered. Please log in or use a different email.";
      } else if (result.errorCode === "WEAK_PASSWORD") {
        validationErrors.value = result.validationErrors || [];
        errorMsg.value = "Password does not meet requirements";
      } else {
        errorMsg.value = result.message || "Registration failed";
        validationErrors.value = result.validationErrors || [];
      }
    }
  } catch (err: any) {
    errorMsg.value = err.message || "Registration failed. Please try again.";
  } finally {
    isLoading.value = false;
  }
}

function handleLoginLink(): void {
  router.push({ name: "Login" });
}
</script>

<template>
  <div class="register-overlay">
    <div class="register-card animate-fade-in">
      <div class="card-logo">
        <svg xmlns="http://www.w3.org/2000/svg" width="32" height="32" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round" class="logo-svg">
          <path d="M14 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8z"></path>
          <polyline points="14 2 14 8 20 8"></polyline>
          <line x1="16" y1="13" x2="8" y2="13"></line>
          <line x1="16" y1="17" x2="8" y2="17"></line>
          <polyline points="10 9 9 9 8 9"></polyline>
        </svg>
      </div>

      <h2 class="welcome-title">Create Account</h2>
      <p class="welcome-subtitle">Join Insight Forge AI Writing Suite</p>

      <div v-if="errorMsg" class="error-alert">
        {{ errorMsg }}
      </div>

      <div v-if="successMsg" class="success-alert">
        {{ successMsg }}
      </div>

      <div v-if="validationErrors.length > 0" class="validation-errors">
        <div v-for="error in validationErrors" :key="error" class="validation-error-item">
          {{ error }}
        </div>
      </div>

      <form @submit.prevent="handleRegister" class="register-form">
        <div class="form-group">
          <label class="form-label" for="email">Email Address</label>
          <input
            type="email"
            id="email"
            placeholder="name@company.com"
            v-model="email"
            class="form-control"
            required
          />
        </div>

        <div class="form-group">
          <label class="form-label" for="password">Password</label>
          <div class="password-input-container">
            <input
              :type="showPassword ? 'text' : 'password'"
              id="password"
              placeholder="••••••••"
              v-model="password"
              @input="handlePasswordInput"
              class="form-control"
              required
            />
            <button
              type="button"
              class="password-toggle-btn"
              @click="showPassword = !showPassword"
              aria-label="Toggle password visibility"
            >
              <svg v-if="showPassword" xmlns="http://www.w3.org/2000/svg" width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                <path d="M17.94 17.94A10.07 10.07 0 0 1 12 20c-7 0-11-8-11-8a18.45 18.45 0 0 1 5.06-5.94M9.9 4.24A9.12 9.12 0 0 1 12 4c7 0 11 8 11 8a18.5 18.5 0 0 1-2.16 3.19m-6.72-1.07a3 3 0 1 1-4.24-4.24"></path>
                <line x1="1" y1="1" x2="23" y2="23"></line>
              </svg>
              <svg v-else xmlns="http://www.w3.org/2000/svg" width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                <path d="M1 12s4-8 11-8 11 8 11 8-4 8-11 8-11-8-11-8z"></path>
                <circle cx="12" cy="12" r="3"></circle>
              </svg>
            </button>
          </div>
          
          <div class="password-strength" v-if="password">
            <div class="strength-bar">
              <div
                class="strength-fill"
                :style="{ width: passwordStrength + '%', backgroundColor: getPasswordStrengthColor() }"
              ></div>
            </div>
            <span class="strength-text" :style="{ color: getPasswordStrengthColor() }">
              {{ passwordStrength < 33 ? "Weak" : passwordStrength < 66 ? "Fair" : "Strong" }}
            </span>
          </div>
        </div>

        <div class="form-group">
          <label class="form-label" for="confirm-password">Confirm Password</label>
          <div class="password-input-container">
            <input
              :type="showConfirmPassword ? 'text' : 'password'"
              id="confirm-password"
              placeholder="••••••••"
              v-model="confirmPassword"
              class="form-control"
              required
            />
            <button
              type="button"
              class="password-toggle-btn"
              @click="showConfirmPassword = !showConfirmPassword"
              aria-label="Toggle password visibility"
            >
              <svg v-if="showConfirmPassword" xmlns="http://www.w3.org/2000/svg" width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                <path d="M17.94 17.94A10.07 10.07 0 0 1 12 20c-7 0-11-8-11-8a18.45 18.45 0 0 1 5.06-5.94M9.9 4.24A9.12 9.12 0 0 1 12 4c7 0 11 8 11 8a18.5 18.5 0 0 1-2.16 3.19m-6.72-1.07a3 3 0 1 1-4.24-4.24"></path>
                <line x1="1" y1="1" x2="23" y2="23"></line>
              </svg>
              <svg v-else xmlns="http://www.w3.org/2000/svg" width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                <path d="M1 12s4-8 11-8 11 8 11 8-4 8-11 8-11-8-11-8z"></path>
                <circle cx="12" cy="12" r="3"></circle>
              </svg>
            </button>
          </div>
        </div>

        <div class="terms-container">
          <input type="checkbox" id="agree-terms" v-model="agreeToTerms" />
          <label for="agree-terms" class="terms-label">
            I agree to the
            <a href="#" @click.prevent class="terms-link">Terms of Service</a>
            and
            <a href="#" @click.prevent class="terms-link">Privacy Policy</a>
          </label>
        </div>

        <button
          type="submit"
          class="btn btn-primary submit-btn"
          :disabled="isLoading || !agreeToTerms"
        >
          <span v-if="isLoading" class="spinner"></span>
          <span v-else>Create Account</span>
        </button>
      </form>

      <p class="login-text">
        Already have an account?
        <a href="#" class="login-link" @click.prevent="handleLoginLink">Log in</a>
      </p>
    </div>
  </div>
</template>

<style scoped>
.register-overlay {
  position: absolute;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background-color: rgba(246, 248, 252, 0.4);
  backdrop-filter: blur(4px);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 100;
}

.dark .register-overlay {
  background-color: rgba(11, 12, 16, 0.5);
}

.register-card {
  width: 440px;
  background-color: var(--bg-card);
  border: 1px solid var(--border-color);
  border-radius: var(--radius-lg);
  box-shadow: var(--shadow-lg);
  padding: var(--space-xl);
  display: flex;
  flex-direction: column;
  align-items: center;
}

.card-logo {
  width: 50px;
  height: 50px;
  background-color: var(--primary-light);
  border-radius: var(--radius-md);
  display: flex;
  align-items: center;
  justify-content: center;
  color: var(--primary-color);
  margin-bottom: var(--space-lg);
}

.welcome-title {
  font-size: var(--font-size-xl);
  font-weight: 700;
  margin-bottom: var(--space-xs);
  color: var(--text-primary);
}

.welcome-subtitle {
  font-size: var(--font-size-sm);
  color: var(--text-secondary);
  margin-bottom: var(--space-lg);
}

.error-alert,
.success-alert,
.validation-errors {
  width: 100%;
  padding: var(--space-sm) var(--space-md);
  border-radius: var(--radius-md);
  margin-bottom: var(--space-md);
  font-size: var(--font-size-sm);
}

.error-alert {
  background-color: rgba(225, 29, 72, 0.08);
  border: 1px solid rgba(225, 29, 72, 0.2);
  color: #e11d48;
}

.success-alert {
  background-color: rgba(34, 197, 94, 0.08);
  border: 1px solid rgba(34, 197, 94, 0.2);
  color: #16a34a;
}

.validation-errors {
  background-color: rgba(225, 29, 72, 0.08);
  border: 1px solid rgba(225, 29, 72, 0.2);
  color: #e11d48;
}

.validation-error-item {
  margin: 4px 0;
}

.register-form {
  width: 100%;
}

.form-group {
  margin-bottom: var(--space-md);
}

.password-input-container {
  position: relative;
}

.password-toggle-btn {
  position: absolute;
  right: 12px;
  top: 50%;
  transform: translateY(-50%);
  background: none;
  border: none;
  color: var(--text-muted);
  cursor: pointer;
  padding: 4px;
}

.password-toggle-btn:hover {
  color: var(--text-secondary);
}

.password-strength {
  margin-top: 8px;
}

.strength-bar {
  height: 4px;
  background-color: var(--bg-subtle);
  border-radius: 2px;
  overflow: hidden;
  margin-bottom: 4px;
}

.strength-fill {
  height: 100%;
  transition: width 0.3s ease, background-color 0.3s ease;
}

.strength-text {
  font-size: 11px;
  font-weight: 600;
  text-transform: uppercase;
  letter-spacing: 0.05em;
}

.terms-container {
  display: flex;
  align-items: flex-start;
  gap: var(--space-sm);
  margin-bottom: var(--space-lg);
}

.terms-label {
  font-size: var(--font-size-sm);
  color: var(--text-secondary);
  cursor: pointer;
  line-height: 1.5;
}

.terms-link {
  color: var(--primary-color);
  text-decoration: none;
  font-weight: 600;
}

.terms-link:hover {
  text-decoration: underline;
}

.submit-btn {
  width: 100%;
  padding: 12px;
  font-size: var(--font-size-base);
}

.login-text {
  font-size: var(--font-size-sm);
  color: var(--text-secondary);
  margin-top: var(--space-lg);
}

.login-link {
  font-weight: 600;
  color: var(--primary-color);
  text-decoration: none;
}

.login-link:hover {
  text-decoration: underline;
}

.spinner {
  width: 20px;
  height: 20px;
  border: 2px solid rgba(255, 255, 255, 0.3);
  border-radius: 50%;
  border-top-color: #ffffff;
  animation: spin 0.8s linear infinite;
}

@keyframes spin {
  to {
    transform: rotate(360deg);
  }
}
</style>
