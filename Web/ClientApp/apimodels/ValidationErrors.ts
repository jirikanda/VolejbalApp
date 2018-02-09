/* tslint:disable */

	export interface ValidationError {
		errors: FieldValidationError[];
		message: string;
		statusCode: number;
	}
	export interface FieldValidationError {
		field: string;
		message: string;
	}
