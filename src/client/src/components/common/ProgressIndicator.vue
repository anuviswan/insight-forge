<script setup lang="ts">
import { computed } from 'vue';
import type { ProgressStep } from '../../stores/documents';

interface Props {
  steps: ProgressStep[];
  currentStep?: string;
}

const props = withDefaults(defineProps<Props>(), {});

const allCompleted = computed(() => props.steps.every(s => s.status === 'completed'));
const hasErrors = computed(() => props.steps.some(s => s.status === 'failed'));
</script>

<template>
  <div class="progress-indicator" v-if="steps.length > 0">
    <div class="steps-container">
      <div v-for="(step, index) in steps" :key="step.id" class="step-item">
        <!-- Step circle -->
        <div :class="['step-circle', step.status]">
          <svg v-if="step.status === 'completed'" class="check-icon" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
            <polyline points="20 6 9 17 4 12"></polyline>
          </svg>
          <svg v-else-if="step.status === 'failed'" class="error-icon" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
            <circle cx="12" cy="12" r="10"></circle>
            <line x1="15" y1="9" x2="9" y2="15"></line>
            <line x1="9" y1="9" x2="15" y2="15"></line>
          </svg>
          <svg v-else-if="step.status === 'in-progress'" class="spinner" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
            <circle cx="12" cy="12" r="10"></circle>
          </svg>
          <span v-else class="step-number">{{ index + 1 }}</span>
        </div>

        <!-- Step info -->
        <div class="step-info">
          <div class="step-label">{{ step.label }}</div>
          <div v-if="step.message" class="step-message">{{ step.message }}</div>
        </div>

        <!-- Connector line (except for last step) -->
        <div v-if="index < steps.length - 1" :class="['connector', { completed: step.status === 'completed' }]"></div>
      </div>
    </div>

    <!-- Status summary -->
    <div class="status-summary" v-if="allCompleted || hasErrors">
      <div v-if="allCompleted" class="success-message">
        <svg class="icon" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
          <polyline points="20 6 9 17 4 12"></polyline>
        </svg>
        <span>All steps completed!</span>
      </div>
      <div v-else-if="hasErrors" class="error-message">
        <svg class="icon" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
          <circle cx="12" cy="12" r="10"></circle>
          <line x1="15" y1="9" x2="9" y2="15"></line>
          <line x1="9" y1="9" x2="15" y2="15"></line>
        </svg>
        <span>Generation failed. Please try again.</span>
      </div>
    </div>
  </div>
</template>

<style scoped>
.progress-indicator {
  background-color: var(--bg-input);
  border-radius: var(--radius-md);
  padding: var(--space-lg);
  margin-bottom: var(--space-lg);
}

.steps-container {
  display: flex;
  flex-direction: column;
  gap: var(--space-lg);
}

.step-item {
  display: flex;
  align-items: flex-start;
  gap: var(--space-md);
  position: relative;
}

.step-circle {
  width: 40px;
  height: 40px;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
  font-weight: 600;
  transition: all var(--transition-fast);
  border: 2px solid var(--border-color);
}

.step-circle.pending {
  background-color: var(--bg-card);
  color: var(--text-secondary);
}

.step-circle.in-progress {
  background-color: var(--primary-color);
  color: white;
  border-color: var(--primary-color);
  animation: pulse 2s ease-in-out infinite;
}

.step-circle.completed {
  background-color: #10b981;
  color: white;
  border-color: #10b981;
}

.step-circle.failed {
  background-color: #ef4444;
  color: white;
  border-color: #ef4444;
}

.step-number {
  font-size: var(--font-size-sm);
}

.check-icon,
.error-icon,
.spinner {
  width: 20px;
  height: 20px;
}

.spinner {
  animation: spin 1s linear infinite;
}

@keyframes spin {
  to { transform: rotate(360deg); }
}

@keyframes pulse {
  0%, 100% { box-shadow: 0 0 0 0 rgba(var(--primary-color-rgb), 0.7); }
  50% { box-shadow: 0 0 0 10px rgba(var(--primary-color-rgb), 0); }
}

.step-info {
  flex: 1;
  padding-top: 2px;
}

.step-label {
  font-weight: 600;
  color: var(--text-primary);
  font-size: var(--font-size-sm);
}

.step-message {
  font-size: var(--font-size-xs);
  color: var(--text-secondary);
  margin-top: 2px;
}

.connector {
  position: absolute;
  left: 19px;
  top: 40px;
  width: 2px;
  height: 40px;
  background-color: var(--border-color);
  transition: background-color var(--transition-fast);
}

.connector.completed {
  background-color: #10b981;
}

.status-summary {
  margin-top: var(--space-lg);
  padding-top: var(--space-lg);
  border-top: 1px solid var(--border-color);
}

.success-message,
.error-message {
  display: flex;
  align-items: center;
  gap: var(--space-sm);
  font-size: var(--font-size-sm);
  font-weight: 500;
}

.success-message {
  color: #10b981;
}

.error-message {
  color: #ef4444;
}

.icon {
  width: 16px;
  height: 16px;
}

@media (max-width: 640px) {
  .progress-indicator {
    padding: var(--space-md);
  }

  .step-item {
    gap: var(--space-sm);
  }

  .step-label {
    font-size: var(--font-size-xs);
  }

  .step-message {
    font-size: 10px;
  }
}
</style>
