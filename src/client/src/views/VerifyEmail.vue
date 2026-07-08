<script setup lang="ts">
import { ref, onMounted } from "vue";
import { useRouter, useRoute } from "vue-router";
import { api } from "../api";

const router = useRouter();
const route = useRoute();

const token = ref("");
const isLoading = ref(true);
const isVerifying = ref(false);
const successMsg = ref("");
const errorMsg = ref("");
const verificationStatus = ref<"pending" | "verifying" | "success" | "error">("pending");

onMounted(async () => {
  // Get token from URL query parameter or route
  token.value = (route.query.token as string) || "";

  if (token.value) {
    isLoading.value = false;
    // Auto-verify if token in URL
    await verifyEmail();
  } else {
    isLoading.value = false;
    errorMsg.value = "No verification token provided. Please check your email for the verification link.";
    verificationStatus.value = "error";
  }
});

async function verifyEmail(): Promise<void> {
  if (!token.value) {
    errorMsg.value = "Verification token is required";
    verificationStatus.value = "error";
    return;
  }

  isVerifying.value = true;
  verificationStatus.value = "verifying";

  try {
    const result = await api.auth.verifyEmail(token.value);

    if (result.success) {
      successMsg.value = result.message || "Email verified successfully! You can now log in.";
      verificationStatus.value = "success";

      setTimeout(() => {
        router.push({ name: "Login" });
      }, 3000);
    } else {
      errorMsg.value =
        result.message ||
        (result.errorCode === "TOKEN_EXPIRED"
          ? "Verification token has expired. Please request a new one."
          : "Email verification failed. Please try again.");
      verificationStatus.value = "error";
    }
  } catch (err: any) {
    errorMsg.value = err.message || "Verification failed. Please try again.";
    verificationStatus.value = "error";
  } finally {
    isVerifying.value = false;
  }
}

function handleLoginClick(): void {
  router.push({ name: "Login" });
}
</script>

<template>
  <div class="verify-overlay">
    <div class="verify-card animate-fade-in">
      <div class="card-logo">
        <svg xmlns="http://www.w3.org/2000/svg" width="32" height="32" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round" class="logo-svg">
          <path d="M14 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8z"></path>
          <polyline points="14 2 14 8 20 8"></polyline>
          <line x1="16" y1="13" x2="8" y2="13"></line>
          <line x1="16" y1="17" x2="8" y2="17"></line>
          <polyline points="10 9 9 9 8 9"></polyline>
        </svg>
      </div>

      <div v-if="isLoading" class="loading-state">
        <div class="spinner large"></div>
        <p class="status-text">Loading verification...</p>
      </div>

      <div v-else-if="verificationStatus === 'verifying'" class="verifying-state">
        <div class="spinner large"></div>
        <h2 class="verify-title">Verifying Email</h2>
        <p class="verify-subtitle">Please wait while we confirm your email address</p>
      </div>

      <div v-else-if="verificationStatus === 'success'" class="success-state">
        <div class="success-icon">
          <svg xmlns="http://www.w3.org/2000/svg" width="64" height="64" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
            <polyline points="20 6 9 17 4 12"></polyline>
          </svg>
        </div>
        <h2 class="verify-title success-title">Email Verified</h2>
        <p class="verify-subtitle success-subtitle">{{ successMsg }}</p>
        <p class="redirect-text">Redirecting to login in 3 seconds...</p>
        <button class="btn btn-primary" @click="handleLoginClick">Go to Login Now</button>
      </div>

      <div v-else-if="verificationStatus === 'error'" class="error-state">
        <div class="error-icon">
          <svg xmlns="http://www.w3.org/2000/svg" width="64" height="64" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
            <circle cx="12" cy="12" r="10"></circle>
            <line x1="15" y1="9" x2="9" y2="15"></line>
            <line x1="9" y1="9" x2="15" y2="15"></line>
          </svg>
        </div>
        <h2 class="verify-title error-title">Verification Failed</h2>
        <p class="verify-subtitle error-subtitle">{{ errorMsg }}</p>
        <div class="button-group">
          <button class="btn btn-primary" @click="handleLoginClick">Go to Login</button>
          <button class="btn btn-secondary" @click="verifyEmail" :disabled="isVerifying">Retry Verification</button>
        </div>
      </div>

      <div v-else class="pending-state">
        <div class="info-icon">
          <svg xmlns="http://www.w3.org/2000/svg" width="64" height="64" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
            <circle cx="12" cy="12" r="10"></circle>
            <line x1="12" y1="16" x2="12" y2="12"></line>
            <line x1="12" y1="8" x2="12.01" y2="8"></line>
          </svg>
        </div>
        <h2 class="verify-title">Verify Your Email</h2>
        <p class="verify-subtitle">Enter your verification token below</p>

        <div class="input-group">
          <input
            v-model="token"
            type="text"
            class="verify-input"
            placeholder="Paste your verification token here"
            @keyup.enter="verifyEmail"
            :disabled="isVerifying"
          />
        </div>

        <p v-if="errorMsg" class="error-message">{{ errorMsg }}</p>

        <div class="button-group">
          <button class="btn btn-primary" @click="verifyEmail" :disabled="isVerifying || !token">
            {{ isVerifying ? "Verifying..." : "Verify Email" }}
          </button>
          <button class="btn btn-secondary" @click="handleLoginClick">Go to Login</button>
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped>
.verify-overlay {
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

.dark .verify-overlay {
  background-color: rgba(11, 12, 16, 0.5);
}

.verify-card {
  width: 440px;
  background-color: var(--bg-card);
  border: 1px solid var(--border-color);
  border-radius: var(--radius-lg);
  box-shadow: var(--shadow-lg);
  padding: var(--space-xl);
  display: flex;
  flex-direction: column;
  align-items: center;
  text-align: center;
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

.verify-title {
  font-size: var(--font-size-xl);
  font-weight: 700;
  margin-bottom: var(--space-xs);
  color: var(--text-primary);
}

.verify-subtitle {
  font-size: var(--font-size-sm);
  color: var(--text-secondary);
  margin-bottom: var(--space-lg);
}

.loading-state,
.verifying-state,
.success-state,
.error-state,
.pending-state {
  display: flex;
  flex-direction: column;
  align-items: center;
  width: 100%;
}

.success-icon,
.error-icon,
.info-icon {
  width: 80px;
  height: 80px;
  margin-bottom: var(--space-lg);
  display: flex;
  align-items: center;
  justify-content: center;
}

.success-icon {
  color: #22c55e;
}

.error-icon {
  color: #ef4444;
}

.info-icon {
  color: var(--primary-color);
}

.success-title {
  color: #22c55e;
}

.error-title {
  color: #ef4444;
}

.success-subtitle {
  color: var(--text-secondary);
}

.error-subtitle {
  color: #dc2626;
}

.status-text {
  color: var(--text-secondary);
  margin-top: var(--space-md);
}

.redirect-text {
  font-size: var(--font-size-sm);
  color: var(--text-secondary);
  margin-bottom: var(--space-lg);
}

.input-group {
  width: 100%;
  margin-bottom: var(--space-md);
}

.verify-input {
  width: 100%;
  padding: var(--space-md);
  border: 1px solid var(--border-color);
  border-radius: var(--radius-md);
  background-color: var(--bg-input);
  color: var(--text-primary);
  font-size: var(--font-size-sm);
  font-family: monospace;
  transition: border-color 0.2s;
}

.verify-input:focus {
  outline: none;
  border-color: var(--primary-color);
  box-shadow: 0 0 0 3px rgba(59, 130, 246, 0.1);
}

.verify-input:disabled {
  background-color: var(--bg-disabled);
  cursor: not-allowed;
  opacity: 0.6;
}

.error-message {
  color: #ef4444;
  font-size: var(--font-size-sm);
  margin-bottom: var(--space-md);
  width: 100%;
}

.button-group {
  display: flex;
  gap: var(--space-md);
  margin-top: var(--space-md);
  width: 100%;
}

.btn {
  flex: 1;
}

.spinner {
  width: 40px;
  height: 40px;
  border: 3px solid rgba(255, 255, 255, 0.3);
  border-radius: 50%;
  border-top-color: var(--primary-color);
  animation: spin 0.8s linear infinite;
}

.spinner.large {
  width: 50px;
  height: 50px;
  border-width: 4px;
}

@keyframes spin {
  to {
    transform: rotate(360deg);
  }
}
</style>
