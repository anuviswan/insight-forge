<script setup lang="ts">
import { computed } from 'vue';
import type { StreamConnectionState } from '../../models/agentStream';

interface Props {
  status: string;
  connectionState: StreamConnectionState;
}

const props = defineProps<Props>();

const connectionLabel = computed(() => {
  switch (props.connectionState) {
    case 'connecting':
      return 'Connecting…';
    case 'reconnecting':
      return 'Reconnecting…';
    case 'connected':
      return 'Live';
    case 'closed':
      return 'Finished';
    case 'failed':
      return 'Connection lost';
    default:
      return 'Idle';
  }
});
</script>

<template>
  <div class="status-display">
    <span :class="['status-dot', connectionState]" aria-hidden="true"></span>
    <div class="status-text">
      <span class="status-message">{{ status || 'Waiting to start…' }}</span>
      <span class="status-connection">{{ connectionLabel }}</span>
    </div>
  </div>
</template>

<style scoped>
.status-display {
  display: flex;
  align-items: center;
  gap: var(--space-sm);
}

.status-dot {
  width: 10px;
  height: 10px;
  border-radius: 50%;
  flex-shrink: 0;
  background-color: var(--text-muted);
}

.status-dot.connected {
  background-color: #10b981;
  animation: pulse-dot 2s ease-in-out infinite;
}

.status-dot.connecting,
.status-dot.reconnecting {
  background-color: #f59e0b;
  animation: pulse-dot 1s ease-in-out infinite;
}

.status-dot.failed {
  background-color: #ef4444;
}

.status-dot.closed {
  background-color: var(--text-muted);
}

@keyframes pulse-dot {
  0%, 100% { opacity: 1; }
  50% { opacity: 0.4; }
}

.status-text {
  display: flex;
  flex-direction: column;
  gap: 2px;
}

.status-message {
  font-size: var(--font-size-sm);
  font-weight: 600;
  color: var(--text-primary);
}

.status-connection {
  font-size: var(--font-size-xs);
  color: var(--text-muted);
}
</style>
