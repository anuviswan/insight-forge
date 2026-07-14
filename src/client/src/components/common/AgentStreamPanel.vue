<script setup lang="ts">
import { toRef } from 'vue';
import { useAgentStream } from '../../composables/useAgentStream';
import StatusDisplay from './StatusDisplay.vue';
import StreamProgressBar from './StreamProgressBar.vue';
import ContentMetrics from './ContentMetrics.vue';
import EventLog from './EventLog.vue';
import ErrorDisplay from './ErrorDisplay.vue';

interface Props {
  jobId: string | null;
}

const props = defineProps<Props>();

const emit = defineEmits<{
  cancel: [];
}>();

const jobIdRef = toRef(props, 'jobId');
const { connectionState, events, latestStatus, latestProgress, latestError, retry } = useAgentStream(jobIdRef);
</script>

<template>
  <div class="agent-stream-panel" v-if="jobId">
    <StatusDisplay :status="latestStatus" :connection-state="connectionState" />

    <StreamProgressBar :progress="latestProgress" />

    <ContentMetrics :progress="latestProgress" />

    <ErrorDisplay v-if="latestError" :error="latestError" @retry="retry" @cancel="emit('cancel')" />

    <EventLog :events="events" />
  </div>
</template>

<style scoped>
.agent-stream-panel {
  display: flex;
  flex-direction: column;
  gap: var(--space-md);
  background-color: var(--bg-card);
  border: 1px solid var(--border-color);
  border-radius: var(--radius-md);
  padding: var(--space-lg);
  margin-bottom: var(--space-lg);
}
</style>
