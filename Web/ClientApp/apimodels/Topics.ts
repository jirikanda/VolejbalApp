/* tslint:disable */

	export interface PutTopicIM {
		name: string;
		validFrom: Date;
		validTo: Date;
	}
	export interface TopicVM {
		hasCustomAttributes: boolean;
		hasChildren: boolean;
		id: number;
		name: string;
		validFrom: Date;
		validTo: Date;
	}
