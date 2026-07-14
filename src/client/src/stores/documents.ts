import { defineStore } from 'pinia';
import { ref, computed } from 'vue';
import { api, type BlogPost, type ResearchSummary } from '../api';

export interface HistoryItem {
  id: string;
  title: string;
  type: 'blog' | 'summary';
  date: string;
}

export interface ProgressStep {
  id: string;
  label: string;
  status: 'pending' | 'in-progress' | 'completed' | 'failed';
  message?: string;
}

const POLL_INTERVAL_MS = 2000;
const MAX_POLL_ATTEMPTS = 300; // 10 minutes at 2s intervals

/** Thrown internally when the user cancels generation; not a real error. */
class GenerationCancelledError extends Error {
  constructor() {
    super('Generation cancelled');
    this.name = 'GenerationCancelledError';
  }
}

export const useDocumentsStore = defineStore('documents', () => {
  const recentHistory = ref<HistoryItem[]>([]);
  const activePost = ref<BlogPost | null>(null);
  const activeSummary = ref<ResearchSummary | null>(null);

  const searchHistoryQuery = ref('');
  const loading = ref(false);
  const error = ref<string | null>(null);
  const progressSteps = ref<ProgressStep[]>([]);
  const currentJobId = ref<string | null>(null);
  const cancelRequested = ref(false);

  const filteredHistory = computed(() => {
    if (!searchHistoryQuery.value.trim()) return recentHistory.value;
    const query = searchHistoryQuery.value.toLowerCase();
    return recentHistory.value.filter(item =>
      item.title.toLowerCase().includes(query)
    );
  });

  async function loadHistory() {
    loading.value = true;
    try {
      const history = await api.history.fetchRecent();
      recentHistory.value = history as HistoryItem[];
    } catch (err: any) {
      error.value = err.message || 'Failed to fetch history';
    } finally {
      loading.value = false;
    }
  }

  function initializeProgress() {
    progressSteps.value = [
      { id: '1', label: 'Research', status: 'pending', message: 'Gathering information...' },
      { id: '2', label: 'Outline', status: 'pending', message: 'Creating structure...' },
      { id: '3', label: 'Write', status: 'pending', message: 'Generating content...' },
      { id: '4', label: 'SEO Optimize', status: 'pending', message: 'Optimizing for search...' },
      { id: '5', label: 'Quality Check', status: 'pending', message: 'Verifying quality...' }
    ];
  }

  function updateProgress(stepId: string, status: ProgressStep['status'], message?: string) {
    const step = progressSteps.value.find(s => s.id === stepId);
    if (step) {
      step.status = status;
      if (message) step.message = message;
    }
  }

  async function generateBlogPost(topic: string, audience: string = '', writingStyle: string = '') {
    loading.value = true;
    error.value = null;
    currentJobId.value = null;
    cancelRequested.value = false;

    try {
      const jobId = await api.blogger.startJob(topic, audience, writingStyle);
      currentJobId.value = jobId;

      const post = await pollJobResult(jobId, topic);
      activePost.value = post;

      // Add to recent history locally
      recentHistory.value.unshift({
        id: Math.random().toString(),
        title: post.title,
        type: 'blog',
        date: 'just now'
      });
      return post;
    } catch (err: any) {
      if (err instanceof GenerationCancelledError) {
        return; // user-initiated cancellation, not a failure - don't surface an error
      }
      error.value = err.message || 'Failed to generate blog post';
      throw err;
    } finally {
      loading.value = false;
    }
  }

  /** Abandon an in-flight generation: stops polling and disconnects the live stream. */
  function cancelGeneration() {
    cancelRequested.value = true;
    currentJobId.value = null;
    loading.value = false;
    error.value = null;
  }

  /** Poll the job result endpoint until the background generation job completes, fails, or is cancelled. */
  async function pollJobResult(jobId: string, topic: string) {
    for (let attempt = 0; attempt < MAX_POLL_ATTEMPTS; attempt++) {
      if (cancelRequested.value) {
        throw new GenerationCancelledError();
      }

      const result = await api.blogger.getResult(jobId, topic);

      if (result.status === 'complete') {
        return result.post;
      }
      if (result.status === 'error') {
        throw new Error(result.error);
      }

      await new Promise(resolve => setTimeout(resolve, POLL_INTERVAL_MS));
    }

    throw new Error('Blog generation timed out. Please try again.');
  }

  async function generateSummary(urls: string[]) {
    loading.value = true;
    error.value = null;
    try {
      const summary = await api.summariser.summarise(urls);
      activeSummary.value = summary;

      // Add to recent history locally
      recentHistory.value.unshift({
        id: Math.random().toString(),
        title: summary.title,
        type: 'summary',
        date: '2 hours ago'
      });
      return summary;
    } catch (err: any) {
      error.value = err.message || 'Failed to generate summary';
      throw err;
    } finally {
      loading.value = false;
    }
  }

  function addEmptyDocument() {
    // Simulates "+ New Document" action
    activePost.value = {
      title: 'Untitled Document',
      content: '# Untitled Document\n\nStart typing here...',
      wordCount: 4,
      readTime: '0m',
      imageUrl: ''
    };
  }

  function deleteHistoryItem(id: string) {
    recentHistory.value = recentHistory.value.filter(item => item.id !== id);
  }

  function clearCurrentJob() {
    currentJobId.value = null;
  }

  return {
    recentHistory,
    activePost,
    activeSummary,
    searchHistoryQuery,
    filteredHistory,
    loading,
    error,
    progressSteps,
    currentJobId,
    loadHistory,
    generateBlogPost,
    generateSummary,
    addEmptyDocument,
    deleteHistoryItem,
    clearCurrentJob,
    cancelGeneration,
    initializeProgress,
    updateProgress
  };
});
