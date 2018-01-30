/* tslint:disable */

	export interface AspPrerenderData {
		openIdConnectSettings: OpenIDConnectSettings;
		webApiUrl: string;
	}
	export interface OpenIDConnectSettings {
		audience: string;
		authority: string;
		clientId: string;
		scope: string;
		silentRenewUrl: string;
	}
