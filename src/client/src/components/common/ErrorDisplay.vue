<script setup lang="ts">
import type { ErrorData } from '../../models/agentStream';

interface Props {
  error: ErrorData;
}

defineProps<Props>();

const emit = defineEmits<{
  retry: [];
  cancel: [];
}>();
</script>

<template>
  <div class="error-display">
    <svg class="error-icon" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
      <circle cx="12" cy="12" r="10"></circle>
      <line x1="12" y1="8" x2="12" y2="12"></line>
      <line x1="12" y1="16" x2="12.01" y2="16"></line>
    </svg>
    <div class="error-content">
      <span class="error-message">{{ error.message }}</span>
      <span class="error-type">{{ error.errorType }}</span>
    </div>
    <div class="error-actions">
      <button v-if="error.retryable" class="btn btn-secondary retry-btn" @click="emit('retry')">
        Retry
      </button>
      <button class="btn btn-secondary cancel-btn" @click="emit('cancel')">
        Cancel
      </button>
    </div>
  </div>
</template>

<style scoped>
.error-display {
  display: flex;
  align-items: center;
  gap: var(--space-md);
  padding: var(--space-md);
  border-radius: var(--radius-md);
  background-color: rgba(239, 68, 68, 0.08);
  border: 1px solid rgba(239, 68, 68, 0.25);
}

.error-icon {
  width: 24px;
  height: 24px;
  color: #ef4444;
  flex-shrink: 0;
}

.error-content {
  display: flex;
  flex-direction: column;
  gap: 2px;
  flex: 1;
}

.error-message {
  font-size: var(--font-size-sm);
  font-weight: 600;
  color: var(--text-primary);
}

.error-type {
  font-size: var(--font-size-xs);
  color: var(--text-muted);
}

.error-actions {
  display: flex;
  gap: var(--space-sm);
  flex-shrink: 0;
}

.retry-btn,
.cancel-btn {
  padding: 6px var(--space-md);
  font-size: var(--font-size-xs);
}
</style>
