/* tslint:disable */

	export interface TopicSearchIM {
		productVariantIds: number[];
		searchText: string;
	}
	export interface TopicSearchVM {
		customAttributeAndCustomAttributeValueOccurencesCountSum: number;
		topicOccurencesCountSum: number;

		/**
		 * Pořadí je náhodné.
		 */
		topicSearchPairs: TopicSearchPair[];
	}
	export interface TopicSearchPair {
		customAttributeAndCustomAttributeValueOccurencesCount: number;
		topicId: number;
	}
