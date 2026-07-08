<script setup lang="ts">
import { ref } from 'vue';
import { useRouter } from 'vue-router';
import { useAuthStore } from '../stores/auth';

const authStore = useAuthStore();
const router = useRouter();

const email = ref('');
const password = ref('');
const showPassword = ref(false);
const rememberMe = ref(false);

const errorMsg = ref('');

async function handleLogin() {
  if (!email.value) {
    errorMsg.value = 'Please enter email address';
    return;
  }

  errorMsg.value = '';
  try {
    await authStore.login(email.value, password.value);
    router.push({ name: 'Blogger' });
  } catch (err: any) {
    errorMsg.value = err.message || 'Login failed. Please check credentials.';
  }
}

async function handleSocialLogin(provider: 'google' | 'github') {
  errorMsg.value = '';
  try {
    await authStore.login('', '', provider);
    router.push({ name: 'Blogger' });
  } catch (err: any) {
    errorMsg.value = `Login with ${provider} failed.`;
  }
}

function handleRegisterLink() {
  router.push({ name: 'Register' });
}
</script>

<template>
  <div class="login-overlay">
    <div class="login-card animate-fade-in">
      <div class="card-logo">
        <svg xmlns="http://www.w3.org/2000/svg" width="32" height="32" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round" class="logo-svg">
          <path d="M14 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8z"></path>
          <polyline points="14 2 14 8 20 8"></polyline>
          <line x1="16" y1="13" x2="8" y2="13"></line>
          <line x1="16" y1="17" x2="8" y2="17"></line>
          <polyline points="10 9 9 9 8 9"></polyline>
        </svg>
      </div>

      <h2 class="welcome-title">Welcome back</h2>
      <p class="welcome-subtitle">Log in to your AI Writing Suite</p>

      <div v-if="errorMsg" class="error-alert">
        {{ errorMsg }}
      </div>

      <form @submit.prevent="handleLogin" class="login-form">
        <div class="form-group">
          <label class="form-label" for="email-address">Email Address</label>
          <input 
            type="email" 
            id="email-address" 
            placeholder="name@company.com" 
            v-model="email"
            class="form-control"
            required
          />
        </div>

        <div class="form-group password-group">
          <div class="password-header">
            <label class="form-label" for="password-field">Password</label>
            <a href="#" class="forgot-link" @click.prevent>Forgot password?</a>
          </div>
          <div class="password-input-container">
            <input 
              :type="showPassword ? 'text' : 'password'" 
              id="password-field" 
              placeholder="••••••••" 
              v-model="password"
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
        </div>

        <div class="remember-me-container">
          <input type="checkbox" id="remember-me" v-model="rememberMe" />
          <label for="remember-me" class="remember-label">Remember me for 30 days</label>
        </div>

        <button type="submit" class="btn btn-primary submit-btn" :disabled="authStore.loading">
          <span v-if="authStore.loading" class="spinner"></span>
          <span v-else>Log In</span>
        </button>
      </form>

      <div class="divider">
        <span class="divider-text">Or continue with</span>
      </div>

      <div class="social-login-grid">
        <button class="btn btn-secondary social-btn" @click="handleSocialLogin('google')">
          <svg viewBox="0 0 24 24" width="18" height="18" class="social-icon">
            <path fill="#ea4335" d="M12 5.04c1.66 0 3.2.57 4.38 1.69l3.27-3.27C17.67 1.58 14.98 1 12 1 7.24 1 3.2 3.73 1.24 7.71l3.87 3C6.03 7.82 8.79 5.04 12 5.04z"/>
            <path fill="#4285f4" d="M23.49 12.27c0-.81-.07-1.59-.2-2.35H12v4.51h6.48c-.29 1.48-1.14 2.73-2.4 3.58v2.98h3.87c2.26-2.09 3.54-5.17 3.54-8.72z"/>
            <path fill="#fbbc05" d="M5.11 14.71c-.24-.71-.37-1.47-.37-2.26s.13-1.55.37-2.26l-3.87-3C.44 8.71 0 10.3 0 12s.44 3.29 1.24 4.82l3.87-3.11z"/>
            <path fill="#34a853" d="M12 23c3.24 0 5.97-1.07 7.96-2.91l-3.87-2.98c-1.1.74-2.5 1.18-4.09 1.18-3.21 0-5.97-2.78-6.89-5.67l-3.87 3C3.2 20.27 7.24 23 12 23z"/>
          </svg>
          Google
        </button>

        <button class="btn btn-secondary social-btn" @click="handleSocialLogin('github')">
          <svg viewBox="0 0 24 24" width="18" height="18" class="social-icon github-icon">
            <path fill="currentColor" d="M12 .297c-6.63 0-12 5.373-12 12 0 5.303 3.438 9.8 8.205 11.385.6.113.82-.258.82-.577 0-.285-.01-1.04-.015-2.04-3.338.724-4.042-1.61-4.042-1.61C4.422 18.07 3.633 17.7 3.633 17.7c-1.087-.744.084-.729.084-.729 1.205.084 1.838 1.236 1.838 1.236 1.07 1.835 2.809 1.305 3.495.998.108-.776.417-1.305.76-1.605-2.665-.3-5.466-1.332-5.466-5.93 0-1.31.465-2.38 1.235-3.22-.135-.303-.54-1.523.105-3.176 0 0 1.005-.322 3.3 1.23.96-.267 1.98-.399 3-.405 1.02.006 2.04.138 3 .405 2.28-1.552 3.285-1.23 3.285-1.23.645 1.653.24 2.873.12 3.176.765.84 1.23 1.91 1.23 3.22 0 4.61-2.805 5.625-5.475 5.92.42.36.81 1.096.81 2.22 0 1.606-.015 2.896-.015 3.286 0 .315.21.69.825.57C20.565 22.092 24 17.592 24 12.297c0-6.627-5.373-12-12-12"/>
          </svg>
          GitHub
        </button>
      </div>

      <p class="signup-text">
        Don't have an account? <a href="#" class="signup-link" @click.prevent="handleRegisterLink">Sign up</a>
      </p>
    </div>
  </div>
</template>

<style scoped>
.login-overlay {
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

.dark .login-overlay {
  background-color: rgba(11, 12, 16, 0.5);
}

.login-card {
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

.error-alert {
  width: 100%;
  padding: var(--space-sm) var(--space-md);
  background-color: rgba(225, 29, 72, 0.08);
  border: 1px solid rgba(225, 29, 72, 0.2);
  color: #e11d48;
  border-radius: var(--radius-md);
  font-size: var(--font-size-sm);
  margin-bottom: var(--space-md);
}

.login-form {
  width: 100%;
}

.password-group {
  margin-bottom: var(--space-md);
}

.password-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.forgot-link {
  font-size: var(--font-size-xs);
  font-weight: 600;
  color: var(--primary-color);
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

.remember-me-container {
  display: flex;
  align-items: center;
  gap: var(--space-sm);
  margin-bottom: var(--space-lg);
}

.remember-label {
  font-size: var(--font-size-sm);
  color: var(--text-secondary);
  cursor: pointer;
}

.submit-btn {
  width: 100%;
  padding: 12px;
  font-size: var(--font-size-base);
}

.divider {
  width: 100%;
  text-align: center;
  border-bottom: 1px solid var(--border-color);
  line-height: 0.1em;
  margin: var(--space-lg) 0;
}

.divider-text {
  background-color: var(--bg-card);
  padding: 0 var(--space-md);
  font-size: var(--font-size-xs);
  color: var(--text-muted);
  text-transform: uppercase;
  font-weight: 700;
  letter-spacing: 0.05em;
}

.social-login-grid {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: var(--space-md);
  width: 100%;
  margin-bottom: var(--space-lg);
}

.social-btn {
  width: 100%;
  display: flex;
  align-items: center;
  justify-content: center;
  background-color: var(--bg-card);
  border: 1px solid var(--border-color);
}

.social-icon {
  margin-right: var(--space-xs);
}

.github-icon {
  color: var(--text-primary);
}

.signup-text {
  font-size: var(--font-size-sm);
  color: var(--text-secondary);
}

.signup-link {
  font-weight: 600;
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
  to { transform: rotate(360deg); }
}
</style>
