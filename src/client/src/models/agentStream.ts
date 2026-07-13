export type AgentEventType =
  | 'Interacting'
  | 'StepStarted'
  | 'StepProgressing'
  | 'StepCompleted'
  | 'InteractionComplete'
  | 'FunctionCalled'
  | 'FunctionCompleted'
  | 'Error';

export interface ProgressData {
  currentStep: number;
  totalSteps?: number;
  wordCount: number;
  characterCount: number;
  paragraphCount: number;
  elapsedTimeSeconds: number;
  totalInputTokens?: number;
  totalOutputTokens?: number;
  totalCachedTokens?: number;
  totalThoughtTokens?: number;
}

export interface ErrorData {
  errorType: string;
  message: string;
  retryable: boolean;
}

export interface AgentStatusEvent {
  eventId: string;
  interactionId: string;
  timestamp: string;
  eventType: AgentEventType;
  status: string;
  progress?: ProgressData;
  error?: ErrorData;
  data?: Record<string, unknown>;
}

export type StreamConnectionState =
  | 'idle'
  | 'connecting'
  | 'connected'
  | 'reconnecting'
  | 'closed'
  | 'failed';
