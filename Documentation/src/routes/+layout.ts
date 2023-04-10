import type { MermaidConfig } from 'mermaid'
import mermaid from 'mermaid'

export const prerender = true;
const mermaidConfig : MermaidConfig = {
    startOnLoad: true,
    theme: 'neutral',
    sequence: {
        mirrorActors: false
    }
}
mermaid.initialize(mermaidConfig);