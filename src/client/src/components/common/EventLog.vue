<script setup lang="ts">
import type { AgentStatusEvent } from '../../models/agentStream';

interface Props {
  events: AgentStatusEvent[];
}

defineProps<Props>();

function formatTime(timestamp: string): string {
  const date = new Date(timestamp);
  if (Number.isNaN(date.getTime())) return '';
  return date.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit', second: '2-digit' });
}
</script>

<template>
  <div class="event-log" v-if="events.length > 0">
    <div class="event-log-list">
      <div v-for="event in events" :key="event.eventId" :class="['event-item', event.eventType.toLowerCase()]">
        <span class="event-time">{{ formatTime(event.timestamp) }}</span>
        <span class="event-type-badge">{{ event.eventType }}</span>
        <span class="event-status">{{ event.status }}</span>
      </div>
    </div>
  </div>
</template>

<style scoped>
.event-log {
  border: 1px solid var(--border-color);
  border-radius: var(--radius-md);
  background-color: var(--bg-input);
}

.event-log-list {
  max-height: 220px;
  overflow-y: auto;
  padding: var(--space-sm);
  display: flex;
  flex-direction: column-reverse;
  gap: 4px;
}

.event-item {
  display: flex;
  align-items: baseline;
  gap: var(--space-sm);
  padding: 4px 6px;
  border-radius: var(--radius-sm);
  font-size: var(--font-size-xs);
}

.event-time {
  color: var(--text-muted);
  font-family: var(--font-mono);
  flex-shrink: 0;
}

.event-type-badge {
  font-weight: 700;
  color: var(--primary-color);
  flex-shrink: 0;
}

.event-item.error .event-type-badge {
  color: #ef4444;
}

.event-item.interactioncomplete .event-type-badge {
  color: #10b981;
}

.event-status {
  color: var(--text-secondary);
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}
</style>
