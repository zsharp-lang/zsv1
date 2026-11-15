import { defineCollection } from 'astro:content';
import { docsLoader } from '@astrojs/starlight/loaders';
import { docsSchema } from '@astrojs/starlight/schema';

// The pages scripts/sync.mjs copies from ../docs, ../design and ../std.
export const collections = {
	docs: defineCollection({ loader: docsLoader(), schema: docsSchema() }),
};
