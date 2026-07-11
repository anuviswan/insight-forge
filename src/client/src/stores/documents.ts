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

export const useDocumentsStore = defineStore('documents', () => {
  const recentHistory = ref<HistoryItem[]>([]);
  const activePost = ref<BlogPost | null>(null);
  const activeSummary = ref<ResearchSummary | null>(null);

  const searchHistoryQuery = ref('');
  const loading = ref(false);
  const error = ref<string | null>(null);
  const progressSteps = ref<ProgressStep[]>([]);

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
    initializeProgress();

    try {
      // Simulate progress updates while API call is in progress
      const progressInterval = setInterval(() => {
        const pending = progressSteps.value.find(s => s.status === 'pending');
        if (pending) {
          pending.status = 'in-progress';
        } else {
          clearInterval(progressInterval);
        }
      }, 2000);

      const post = await api.blogger.generate(topic, audience, writingStyle);
      clearInterval(progressInterval);

      // Mark all steps as completed
      progressSteps.value.forEach(step => {
        step.status = 'completed';
      });

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
      error.value = err.message || 'Failed to generate blog post';
      progressSteps.value.forEach(step => {
        if (step.status === 'in-progress' || step.status === 'pending') {
          step.status = 'failed';
        }
      });
      throw err;
    } finally {
      loading.value = false;
    }
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

  return {
    recentHistory,
    activePost,
    activeSummary,
    searchHistoryQuery,
    filteredHistory,
    loading,
    error,
    progressSteps,
    loadHistory,
    generateBlogPost,
    generateSummary,
    addEmptyDocument,
    deleteHistoryItem,
    initializeProgress,
    updateProgress
  };
});
