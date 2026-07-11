---
description: Unified workflow for research-driven blog post generation with content quality requirements
---

# Technical Blogger - Unified Workflow

## Purpose

Generate comprehensive, high-quality technical blog posts with integrated research, domain analysis, and content quality optimization in a single end-to-end workflow.

## Input

- Topic: Subject to research and write about
- Audience: Target reader profile (Beginners, Intermediate, Experts/Architects)
- WritingStyle: Tone to use (Professional, Casual, Technical)

## Workflow Steps

### Step 1 - Research
**Description:** Conduct extensive research on the topic

**Responsibilities:**
- Gather comprehensive information from multiple sources
- Ask clarifying questions if details are missing
- NEVER make assumptions
- Document key findings, best practices, code examples, common pitfalls
- Collect references and sources

**Skills Required:** research

**Output:** Research Artifacts

---

### Step 2 - Domain Analysis
**Description:** Analyze research findings and provide expert feedback

**Responsibilities:**
- Extract key findings and takeaways from research
- Identify if code examples are needed (and which languages)
- Flag if architectural diagrams would help understanding
- Note common pitfalls and best practices to highlight
- Recommend structure for executive summary and table of contents
- For .Net topics: use dotnet-evangelist skill for domain expertise

**Output:** Domain Analysis Artifacts

---

### Step 3 - Create Outline
**Description:** Transform research into logical article structure

**Responsibilities:**
- Create clear, compelling title
- Organize sections from simple to complex
- Include all research findings in logical flow
- Plan executive summary section
- Structure table of contents
- Identify sections requiring code snippets or diagrams

**Skills Required:** outline

**Output:** Article Outline

---

### Step 4 - Write Draft
**Description:** Generate initial draft with content quality requirements

**Structure Requirements:**
- Clear, descriptive blog title
- Executive summary (2-3 paragraphs with key takeaways)
- Table of contents
- Proper heading hierarchy (H2 for sections, H3 for subsections)

**Content Enhancements:**
- Code snippets with language tags when demonstrating concepts
- Mermaid diagrams for architecture/workflows/relationships
- Research findings with proper attribution
- References section citing sources

**Audience Adaptation:**
- Adjust technical depth for target audience
- Use appropriate examples and terminology
- Follow specified writing style

**Skills Required:** writer

**Output:** Draft Article

---

### Step 5 - SEO Optimization
**Description:** Optimize article for search engines

**Skills Required:** seo

**Output:** SEO Optimized Article

---

### Step 6 - Originality Check
**Description:** Review for originality and refine content

**Process:**
- Review article for originality
- Use feedback to refine as needed
- Repeat until originality check succeeds

**Skills Required:** originality-check

---

### Step 7 - Final Output
**Description:** Produce the final blog post

**Deliverables:**
- Complete, production-ready blog post
- Well-formatted Markdown
- All content quality requirements met

---

## Quality Gate - Content Requirements (PR3)

All generated blogs MUST include:

✅ **Structure**
- Clear, descriptive title (#30)
- Executive summary with key takeaways (#31)
- Table of contents (#32)
- Appropriate section headings (H2, H3) (#33)

✅ **Content Elements**
- Code snippets with language tags (#36)
- Mermaid diagrams when appropriate (#37)
- References section
- Proper attribution of research

---

## Benefits of Unified Workflow

- **Single Round Trip:** Research and blog generation in one API call
- **Optimized Token Usage:** No redundant research processing
- **Integrated Quality:** Research findings inform every step
- **Professional Output:** Consistent application of content quality standards
- **Traceable Content:** Clear lineage from research to final blog
