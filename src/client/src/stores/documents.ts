import { defineStore } from 'pinia';
import { ref, computed } from 'vue';
import { api, type BlogPost, type ResearchSummary } from '../api';

export interface HistoryItem {
  id: string;
  title: string;
  type: 'blog' | 'summary';
  date: string;
}

export const useDocumentsStore = defineStore('documents', () => {
  const recentHistory = ref<HistoryItem[]>([]);
  const activePost = ref<BlogPost | null>(null);
  const activeSummary = ref<ResearchSummary | null>(null);
  
  const searchHistoryQuery = ref('');
  const loading = ref(false);
  const error = ref<string | null>(null);

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

  async function generateBlogPost(topic: string, audience: string = '', writingStyle: string = '') {
    loading.value = true;
    error.value = null;
    try {
      const post = await api.blogger.generate(topic, audience, writingStyle);
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
    loadHistory,
    generateBlogPost,
    generateSummary,
    addEmptyDocument,
    deleteHistoryItem
  };
});
