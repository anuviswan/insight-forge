<script setup lang="ts">
import { ref } from 'vue';
import { useDocumentsStore } from '../stores/documents';

const documentsStore = useDocumentsStore();

const rawUrls = ref(`https://example.com/article-1
https://example.com/research-paper-2`);

async function handleSummarise() {
  const urlList = rawUrls.value
    .split('\n')
    .map(u => u.trim())
    .filter(u => u.length > 0);

  if (urlList.length === 0) return;
  
  try {
    await documentsStore.generateSummary(urlList);
  } catch (err) {
    console.error('Failed to generate summary:', err);
  }
}

function handleCopy() {
  if (documentsStore.activeSummary) {
    navigator.clipboard.writeText(documentsStore.activeSummary.content);
    alert('Summary copied to clipboard!');
  }
}

function handleDownload() {
  if (documentsStore.activeSummary) {
    const blob = new Blob([documentsStore.activeSummary.content], { type: 'text/markdown' });
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = `${documentsStore.activeSummary.title.toLowerCase().replace(/\s+/g, '-')}.md`;
    document.body.appendChild(a);
    a.click();
    document.body.removeChild(a);
    URL.revokeObjectURL(url);
  }
}
</script>

<template>
  <div class="summariser-container animate-fade-in">
    <!-- URLs Input Form -->
    <div class="urls-card generator-card card">
      <h1 class="generator-title">Research Summariser</h1>
      <p class="generator-subtitle">Distill multiple web sources into a single markdown summary.</p>
      <div class="urls-header">
        <div class="urls-label-left">
          <svg xmlns="http://www.w3.org/2000/svg" width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" class="link-icon">
            <path d="M10 13a5 5 0 0 0 7.54.54l3-3a5 5 0 0 0-7.07-7.07l-1.72 1.71"></path>
            <path d="M14 11a5 5 0 0 0-7.54-.54l-3 3a5 5 0 0 0 7.07 7.07l1.71-1.71"></path>
          </svg>
          <span class="urls-title">SOURCE URLS</span>
        </div>
        <span class="urls-subtext text-muted">Paste one link per line</span>
      </div>

      <div class="textarea-container">
        <textarea 
          v-model="rawUrls" 
          rows="4" 
          placeholder="https://example.com/article-1..." 
          class="form-control urls-textarea"
          :disabled="documentsStore.loading"
        ></textarea>
        <div class="textarea-plus-btn">
          <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round">
            <line x1="12" y1="5" x2="12" y2="19"></line>
            <line x1="5" y1="12" x2="19" y2="12"></line>
          </svg>
        </div>
      </div>

      <div class="summarise-btn-container">
        <button class="btn btn-primary summarise-btn" @click="handleSummarise" :disabled="documentsStore.loading || !rawUrls.trim()">
          <span v-if="documentsStore.loading" class="spinner"></span>
          <template v-else>
            <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
              <polygon points="12 2 15.09 8.26 22 9.27 17 14.14 18.18 21.02 12 17.77 5.82 21.02 7 14.14 2 9.27 8.91 8.26 12 2"></polygon>
            </svg>
            Summarise
          </template>
        </button>
      </div>
    </div>

    <!-- Bottom Dashboard (Markdown preview + sidebars) -->
    <div class="summariser-dashboard" v-if="documentsStore.activeSummary">
      <!-- Left side: Markdown Preview -->
      <div class="markdown-preview-column">
        <div class="preview-card card">
          <div class="preview-card-header">
            <div class="header-left-title">
              <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round" class="doc-icon">
                <path d="M14 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8z"></path>
                <polyline points="14 2 14 8 20 8"></polyline>
              </svg>
              <span class="preview-title-text">MARKDOWN PREVIEW</span>
            </div>
            <button class="icon-only-btn" @click="handleCopy" aria-label="Copy summary text">
              <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                <rect x="9" y="9" width="13" height="13" rx="2" ry="2"></rect>
                <path d="M5 15H4a2 2 0 0 1-2-2V4a2 2 0 0 1 2-2h9a2 2 0 0 1 2 2v1"></path>
              </svg>
            </button>
          </div>

          <div class="preview-card-content">
            <h2 class="content-main-title">{{ documentsStore.activeSummary.title }}</h2>
            
            <h3 class="content-section-title">Executive Overview</h3>
            <p class="content-paragraph">This report synthesises information from provided sources regarding the trajectory of quantum processor development and its implications for cryptographic standards. Recent breakthroughs indicate a shortening timeline for practical quantum advantage in specific chemical simulation domains.</p>
            
            <h3 class="content-section-title">Key Insights & Bullet Points</h3>
            <ul class="content-bullets">
              <li><strong>Error Mitigation:</strong> New hybrid algorithms are reducing logical qubit overhead by 30% compared to 2023 benchmarks.</li>
              <li><strong>Hardware Scaling:</strong> Superconducting circuits remain the dominant architecture, though trapped-ion systems are showing superior coherence times.</li>
              <li><strong>Market Impact:</strong> Investment in quantum-safe encryption is projected to grow by 40% annually through 2028.</li>
            </ul>

            <h3 class="content-section-title">Technical Constraints</h3>
            <p class="content-paragraph">Cryogenic requirements remain the primary bottleneck for edge deployment. Current modular cooling units are still too voluminous for data centre rack standards, necessitating bespoke facility designs.</p>

            <h3 class="content-section-title">Next Steps</h3>
            <p class="content-paragraph">Researchers should focus on cross-platform verification and the standardisation of quantum assembly languages (QASM) to ensure inter-operability between hardware vendors.</p>
          </div>
        </div>
      </div>

      <!-- Right side: Sidebar metrics and widgets -->
      <div class="metrics-column">
        <!-- Export Options -->
        <div class="metrics-card card">
          <span class="card-meta-title">EXPORT OPTIONS</span>
          <div class="export-buttons-stack">
            <button class="btn btn-secondary export-btn" @click="handleDownload">
              <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                <path d="M21 15v4a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2v-4"></path>
                <polyline points="7 10 12 15 17 10"></polyline>
                <line x1="12" y1="15" x2="12" y2="3"></line>
              </svg>
              <span>Download .md</span>
              <span class="btn-badge">{{ documentsStore.activeSummary.sizeKb }} KB</span>
            </button>
            <button class="btn btn-secondary export-btn" @click="handleCopy">
              <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                <rect x="9" y="9" width="13" height="13" rx="2" ry="2"></rect>
                <path d="M5 15H4a2 2 0 0 1-2-2V4a2 2 0 0 1 2-2h9a2 2 0 0 1 2 2v1"></path>
              </svg>
              <span>Copy to Clipboard</span>
            </button>
          </div>
        </div>

        <!-- Analysis Meta -->
        <div class="metrics-card card">
          <span class="card-meta-title">ANALYSIS META</span>
          <div class="meta-row">
            <span class="meta-label">Sources Analysed</span>
            <span class="meta-value">{{ documentsStore.activeSummary.sourcesAnalyzed }}</span>
          </div>
          <div class="meta-divider"></div>
          <div class="meta-row">
            <span class="meta-label">Reading Time Saved</span>
            <span class="meta-value highlight-green">{{ documentsStore.activeSummary.readingTimeSaved }} min</span>
          </div>
        </div>

        <!-- AI Engine Spec Card -->
        <div class="engine-spec-card">
          <div class="spec-content">
            <span class="engine-title">AI Analysis Engine v4.2</span>
            <span class="engine-context">Context Window: 128k Tokens</span>
          </div>
          <!-- Abstract mesh vector background using inline SVG -->
          <div class="svg-background">
            <svg viewBox="0 0 200 100" fill="none" xmlns="http://www.w3.org/2000/svg" class="geometric-grid">
              <path d="M0 80 Q 50 10, 100 80 T 200 40" stroke="rgba(255, 255, 255, 0.15)" stroke-width="1.5" />
              <path d="M0 50 Q 70 90, 130 20 T 200 70" stroke="rgba(255, 255, 255, 0.1)" stroke-width="1" />
            </svg>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped>

.generator-card {
  text-align: center;
  padding: var(--space-xl);
}

.generator-title {
  font-size: var(--font-size-xxl);
  font-weight: 700;
  margin-bottom: var(--space-xs);
}

.generator-subtitle {
  font-size: var(--font-size-sm);
  color: var(--text-secondary);
  margin-bottom: var(--space-lg);
}

.summariser-container {
  max-width: 900px;
  margin: 0 auto;
  width: 100%;
  display: flex;
  flex-direction: column;
  gap: var(--space-lg);
  padding-bottom: 80px;
}

.summariser-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.page-title {
  font-size: var(--font-size-xl);
  font-weight: 700;
  margin-bottom: 2px;
}

.page-subtitle {
  font-size: var(--font-size-sm);
}

.header-actions {
  display: flex;
  gap: var(--space-sm);
}

.urls-card {
  padding: var(--space-lg);
}

.urls-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: var(--space-sm);
}

.urls-label-left {
  display: flex;
  align-items: center;
  gap: var(--space-xs);
  color: var(--text-secondary);
}

.link-icon {
  color: var(--primary-color);
}

.urls-title {
  font-size: var(--font-size-xs);
  font-weight: 700;
  letter-spacing: 0.05em;
}

.urls-subtext {
  font-size: var(--font-size-xs);
  font-weight: 600;
}

.textarea-container {
  position: relative;
}

.urls-textarea {
  resize: vertical;
  background-color: var(--bg-input);
  border-radius: var(--radius-md);
  padding-right: 40px;
  line-height: 1.6;
}

.textarea-plus-btn {
  position: absolute;
  right: 12px;
  bottom: 12px;
  color: var(--text-muted);
  cursor: pointer;
  padding: 4px;
  border-radius: 4px;
  background-color: var(--bg-card);
  border: 1px solid var(--border-color);
  display: flex;
  align-items: center;
  justify-content: center;
  transition: all var(--transition-fast);
}

.textarea-plus-btn:hover {
  color: var(--text-primary);
  border-color: var(--text-muted);
}

.summarise-btn-container {
  margin-top: var(--space-md);
  display: flex;
  justify-content: flex-end;
}

.summarise-btn {
  padding: var(--space-sm) var(--space-xl);
  font-size: var(--font-size-sm);
}

.summariser-dashboard {
  display: grid;
  grid-template-columns: 1.8fr 1fr;
  gap: var(--space-lg);
}

.preview-card {
  padding: 0;
  overflow: hidden;
}

.preview-card-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: var(--space-md) var(--space-lg);
  border-bottom: 1px solid var(--border-color);
  background-color: var(--bg-input);
}

.header-left-title {
  display: flex;
  align-items: center;
  gap: var(--space-sm);
  color: var(--text-secondary);
}

.preview-title-text {
  font-family: var(--font-mono);
  font-weight: 700;
  font-size: var(--font-size-xs);
  letter-spacing: 0.05em;
}

.icon-only-btn {
  background: none;
  border: none;
  color: var(--text-secondary);
  cursor: pointer;
  padding: 4px;
  border-radius: var(--radius-sm);
  transition: all var(--transition-fast);
}

.icon-only-btn:hover {
  background-color: var(--border-color);
  color: var(--text-primary);
}

.preview-card-content {
  padding: var(--space-xl);
  line-height: 1.7;
}

.content-main-title {
  font-size: var(--font-size-xl);
  font-weight: 700;
  margin-bottom: var(--space-lg);
  color: var(--text-primary);
}

.content-section-title {
  font-size: var(--font-size-base);
  font-weight: 700;
  margin-top: var(--space-lg);
  margin-bottom: var(--space-sm);
  color: var(--primary-color);
}

.content-paragraph {
  margin-bottom: var(--space-md);
  color: var(--text-secondary);
}

.content-bullets {
  margin-bottom: var(--space-md);
  padding-left: var(--space-lg);
  color: var(--text-secondary);
}

.content-bullets li {
  margin-bottom: var(--space-xs);
}

.metrics-column {
  display: flex;
  flex-direction: column;
  gap: var(--space-lg);
}

.metrics-card {
  padding: var(--space-md) var(--space-lg);
}

.card-meta-title {
  font-size: var(--font-size-xs);
  font-weight: 700;
  text-transform: uppercase;
  letter-spacing: 0.05em;
  color: var(--text-muted);
  display: block;
  margin-bottom: var(--space-md);
}

.export-buttons-stack {
  display: flex;
  flex-direction: column;
  gap: var(--space-sm);
}

.export-btn {
  justify-content: flex-start;
  padding: 12px var(--space-md);
  width: 100%;
}

.btn-badge {
  margin-left: auto;
  font-size: 10px;
  background-color: var(--primary-light);
  color: var(--primary-color);
  padding: 2px 6px;
  border-radius: var(--radius-sm);
  font-weight: 700;
}

.meta-row {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: var(--space-sm) 0;
}

.meta-label {
  font-size: var(--font-size-sm);
  color: var(--text-secondary);
  font-weight: 500;
}

.meta-value {
  font-size: var(--font-size-base);
  font-weight: 700;
}

.meta-divider {
  height: 1px;
  background-color: var(--border-color);
}

.highlight-green {
  color: #10b981;
}

/* Premium gradient spec card */
.engine-spec-card {
  position: relative;
  background: linear-gradient(135deg, #1e1b4b, #312e81);
  border-radius: var(--radius-lg);
  padding: var(--space-lg);
  color: #ffffff;
  overflow: hidden;
  height: 120px;
  display: flex;
  align-items: flex-end;
  border: 1px solid rgba(255, 255, 255, 0.08);
  box-shadow: var(--shadow-lg);
}

.spec-content {
  display: flex;
  flex-direction: column;
  z-index: 2;
}

.engine-title {
  font-size: var(--font-size-sm);
  font-weight: 700;
  letter-spacing: 0.02em;
}

.engine-context {
  font-size: 11px;
  opacity: 0.7;
}

.svg-background {
  position: absolute;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  z-index: 1;
}

.geometric-grid {
  width: 100%;
  height: 100%;
  opacity: 0.6;
}

.spinner {
  width: 18px;
  height: 18px;
  border: 2px solid rgba(255, 255, 255, 0.3);
  border-radius: 50%;
  border-top-color: #ffffff;
  animation: spin 0.8s linear infinite;
}

@keyframes spin {
  to { transform: rotate(360deg); }
}
</style>
