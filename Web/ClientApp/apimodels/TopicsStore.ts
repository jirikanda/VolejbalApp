/* tslint:disable */

	export interface CustomAttributeIM {
		includedInQuickSelection: boolean;
		measureUnitId: number;
		name: string;
		topicId: number;
		type: CustomAttributeType;
		usesSatisfactionRate: boolean;
	}
	export interface CustomAttributeMovementIM {

		/**
		 * Id přesouvané upřesňující podmínky
		 */
		id: number;

		/**
		 * Před jakou upřesňující podmínku přesouvám
		 */
		targetBeforeCustomAttributeId: number;

		/**
		 * Pod jaké téma přesouvám
		 */
		targetParentId: number;
	}
	export interface CustomAttributeVM {
		id: number;
		includedInQuickSelection: boolean;
		measureUnitId: number;
		name: string;
		topicId: number;
		type: CustomAttributeType;
		usesSatisfactionRate: boolean;
	}

	/**
	 * Parametry pro poskytnutí dat pro zafiltrování Témat.
	 */
	export interface ExistsPersonalTopicSelectionNameIM {
		topicSelectionName: string;
	}
	export interface PersonalTopicSelectionVM {
		topicSelectionId: number;
		topicSelectionName: string;
	}

	/**
	 * Parametry pro poskytnutí dat pro zafiltrování Témat.
	 */
	export interface PostPersonalTopicSelectionIM {
		topics: number[];
		topicSelectionName: string;
	}
	export interface PostTopicIM {
		name: string;

		/**
		 * null - chceme použít i pro založení nového topicu na root úrovni
		 */
		parentTopicId: number;
		validFrom: Date;
		validTo: Date;
	}
	export interface PostTopicMovementIM {

		/**
		 * Id přesouvaného tématu
		 */
		id: number;

		/**
		 * Před jaké téma přesouvám
		 */
		targetBeforeTopicId: number;

		/**
		 * Pod jaké téma přesouvám
		 */
		targetParentId: number;
	}
	export interface TopicSearchPair {
		customAttributeAndCustomAttributeValueOccurencesCount: number;
		topicId: number;
	}
	export interface TopicsVM {
		topics: Topic[];
	}
	export interface CustomAttribute {
		id: number;
		name: string;
	}
	export interface Topic {
		customAttributes: CustomAttribute[];
		children: Topic[];
		id: number;
		isActive: boolean;
		name: string;
		uiOrder: number;
	}
	export enum CustomAttributeType {
		Text = 1,
		Integer = 2,
		Decimal = 3,
		Boolean = 4,
		BooleanExtended = 5,
		Date = 6
	}
