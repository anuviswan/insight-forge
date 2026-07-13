<script setup lang="ts">
import { computed } from 'vue';
import type { ProgressData } from '../../models/agentStream';

interface Props {
  progress: ProgressData | null;
}

const props = defineProps<Props>();

const elapsedFormatted = computed(() => {
  const seconds = Math.floor(props.progress?.elapsedTimeSeconds ?? 0);
  const mins = Math.floor(seconds / 60);
  const secs = seconds % 60;
  return `${mins}:${secs.toString().padStart(2, '0')}`;
});

const totalTokens = computed(() => {
  const input = props.progress?.totalInputTokens ?? 0;
  const output = props.progress?.totalOutputTokens ?? 0;
  return input + output;
});

const hasTokens = computed(() => (props.progress?.totalInputTokens ?? 0) > 0 || (props.progress?.totalOutputTokens ?? 0) > 0);
</script>

<template>
  <div class="content-metrics" v-if="progress">
    <div class="metric">
      <span class="metric-value">{{ progress.wordCount }}</span>
      <span class="metric-label">Words</span>
    </div>
    <div class="metric">
      <span class="metric-value">{{ progress.paragraphCount }}</span>
      <span class="metric-label">Paragraphs</span>
    </div>
    <div class="metric">
      <span class="metric-value">{{ elapsedFormatted }}</span>
      <span class="metric-label">Elapsed</span>
    </div>
    <div class="metric" v-if="hasTokens">
      <span class="metric-value">{{ totalTokens.toLocaleString() }}</span>
      <span class="metric-label">Tokens</span>
    </div>
  </div>
</template>

<style scoped>
.content-metrics {
  display: flex;
  gap: var(--space-lg);
  flex-wrap: wrap;
}

.metric {
  display: flex;
  flex-direction: column;
  gap: 2px;
}

.metric-value {
  font-size: var(--font-size-base);
  font-weight: 700;
  color: var(--text-primary);
  font-family: var(--font-mono);
}

.metric-label {
  font-size: var(--font-size-xs);
  color: var(--text-muted);
  text-transform: uppercase;
  letter-spacing: 0.03em;
}
</style>
