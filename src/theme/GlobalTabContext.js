import React from 'react';
import { DEFAULT_LANGUAGE } from './codeTabs';

export const GlobalTabContext = React.createContext({
	value: DEFAULT_LANGUAGE,
	setValue: () => {},
});
