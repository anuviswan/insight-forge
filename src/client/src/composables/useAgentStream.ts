import { ref, computed, watch, onUnmounted, type Ref } from 'vue';
import { API_BASE_URL } from '../api';
import type {
  AgentStatusEvent,
  ProgressData,
  ErrorData,
  StreamConnectionState
} from '../models/agentStream';

const MAX_EVENT_HISTORY = 50;
const MAX_RECONNECT_ATTEMPTS = 5;
const INITIAL_RECONNECT_DELAY_MS = 1000;

/**
 * Connects to the agent status SSE stream for a given job and exposes
 * live connection state, progress, and event history as reactive refs.
 */
export function useAgentStream(jobId: Ref<string | null>) {
  const connectionState = ref<StreamConnectionState>('idle');
  const events = ref<AgentStatusEvent[]>([]);
  const latestEvent = ref<AgentStatusEvent | null>(null);
  const latestProgress = ref<ProgressData | null>(null);
  const latestError = ref<ErrorData | null>(null);
  const isComplete = ref(false);
  const reconnectAttempt = ref(0);

  let eventSource: EventSource | null = null;
  let reconnectTimer: ReturnType<typeof setTimeout> | null = null;

  const latestStatus = computed(() => latestEvent.value?.status ?? '');
  const hasError = computed(() => latestError.value !== null);

  function clearReconnectTimer() {
    if (reconnectTimer !== null) {
      clearTimeout(reconnectTimer);
      reconnectTimer = null;
    }
  }

  function reset() {
    events.value = [];
    latestEvent.value = null;
    latestProgress.value = null;
    latestError.value = null;
    isComplete.value = false;
    reconnectAttempt.value = 0;
  }

  function closeEventSource() {
    clearReconnectTimer();
    if (eventSource) {
      eventSource.close();
      eventSource = null;
    }
  }

  function handleMessage(rawEvent: MessageEvent) {
    let parsed: AgentStatusEvent;
    try {
      parsed = JSON.parse(rawEvent.data);
    } catch (err) {
      console.error('Failed to parse agent status event', err);
      return;
    }

    if (!parsed || !parsed.eventType) {
      console.error('Received malformed agent status event', rawEvent.data);
      return;
    }

    latestEvent.value = parsed;
    events.value.push(parsed);
    if (events.value.length > MAX_EVENT_HISTORY) {
      events.value.shift();
    }

    if (parsed.progress) {
      latestProgress.value = parsed.progress;
    }

    if (parsed.eventType === 'Error') {
      latestError.value = parsed.error ?? {
        errorType: 'Unknown',
        message: parsed.status || 'An unknown error occurred',
        retryable: false
      };

      if (!latestError.value.retryable) {
        isComplete.value = true;
        closeEventSource();
        connectionState.value = 'closed';
      }
      return;
    }

    if (parsed.eventType === 'InteractionComplete') {
      isComplete.value = true;
      closeEventSource();
      connectionState.value = 'closed';
    }
  }

  function scheduleReconnect() {
    if (reconnectAttempt.value >= MAX_RECONNECT_ATTEMPTS) {
      connectionState.value = 'failed';
      return;
    }

    connectionState.value = 'reconnecting';
    const delay = INITIAL_RECONNECT_DELAY_MS * Math.pow(2, reconnectAttempt.value);
    reconnectAttempt.value += 1;

    reconnectTimer = setTimeout(connect, delay);
  }

  function connect() {
    if (!jobId.value || isComplete.value) return;

    closeEventSource();
    connectionState.value = reconnectAttempt.value > 0 ? 'reconnecting' : 'connecting';

    const source = new EventSource(`${API_BASE_URL}/agent/blog/${jobId.value}/stream`);
    eventSource = source;

    source.onopen = () => {
      connectionState.value = 'connected';
      reconnectAttempt.value = 0;
    };

    source.onmessage = handleMessage;

    source.onerror = () => {
      if (isComplete.value) {
        closeEventSource();
        connectionState.value = 'closed';
        return;
      }
      closeEventSource();
      scheduleReconnect();
    };
  }

  function disconnect() {
    closeEventSource();
    connectionState.value = 'closed';
  }

  /** Reset error state and manually retry the connection. */
  function retry() {
    latestError.value = null;
    isComplete.value = false;
    reconnectAttempt.value = 0;
    connect();
  }

  watch(
    jobId,
    (newJobId, oldJobId) => {
      if (newJobId === oldJobId) return;
      closeEventSource();
      reset();
      connectionState.value = 'idle';
      if (newJobId) {
        connect();
      }
    },
    { immediate: true }
  );

  onUnmounted(() => {
    closeEventSource();
  });

  return {
    connectionState,
    events,
    latestEvent,
    latestProgress,
    latestStatus,
    latestError,
    hasError,
    isComplete,
    connect,
    disconnect,
    retry
  };
}
