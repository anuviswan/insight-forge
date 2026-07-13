<script setup lang="ts">
import { computed } from 'vue';
import type { ProgressData } from '../../models/agentStream';

interface Props {
  progress: ProgressData | null;
}

const props = defineProps<Props>();

const hasSteps = computed(() => !!props.progress?.totalSteps);

const percentage = computed(() => {
  if (!props.progress?.totalSteps) return 0;
  return Math.min(100, Math.round(((props.progress.currentStep + 1) / props.progress.totalSteps) * 100));
});

const stepLabel = computed(() => {
  if (!props.progress) return '';
  const current = props.progress.currentStep + 1;
  return props.progress.totalSteps ? `Step ${current} of ${props.progress.totalSteps}` : `Step ${current}`;
});
</script>

<template>
  <div class="stream-progress-bar" v-if="progress">
    <div class="progress-header">
      <span class="progress-label">{{ stepLabel }}</span>
      <span v-if="hasSteps" class="progress-percentage">{{ percentage }}%</span>
    </div>
    <div class="progress-track" role="progressbar" :aria-valuenow="percentage" aria-valuemin="0" aria-valuemax="100">
      <div class="progress-fill" :class="{ indeterminate: !hasSteps }" :style="hasSteps ? { width: percentage + '%' } : {}"></div>
    </div>
  </div>
</template>

<style scoped>
.stream-progress-bar {
  display: flex;
  flex-direction: column;
  gap: var(--space-xs);
}

.progress-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.progress-label {
  font-size: var(--font-size-xs);
  font-weight: 600;
  color: var(--text-secondary);
}

.progress-percentage {
  font-size: var(--font-size-xs);
  font-weight: 600;
  color: var(--primary-color);
}

.progress-track {
  height: 8px;
  border-radius: var(--radius-full);
  background-color: var(--bg-input);
  overflow: hidden;
}

.progress-fill {
  height: 100%;
  background-color: var(--primary-color);
  border-radius: var(--radius-full);
  transition: width 0.3s ease;
}

.progress-fill.indeterminate {
  width: 40%;
  animation: indeterminate-slide 1.4s ease-in-out infinite;
}

@keyframes indeterminate-slide {
  0% { transform: translateX(-100%); }
  100% { transform: translateX(250%); }
}
</style>
