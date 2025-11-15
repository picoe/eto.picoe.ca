import React, { useContext } from 'react';
import { useLocation } from '@docusaurus/router';
import clsx from 'clsx';
import { GlobalTabContext } from '../../GlobalTabContext';

const LANGUAGE_OPTIONS = [
	{ value: 'csharp', label: 'C#' },
	{ value: 'vbnet', label: 'VB' },
	{ value: 'fsharp', label: 'F#' },
	{ value: 'python', label: 'Python' }
];

export default function LanguageSelectorNavbarItem({ className }) {
	const { value, setValue } = useContext(GlobalTabContext);
	const location = useLocation();
	const pathname = location?.pathname ?? '';

	if (!pathname.startsWith('/api')) {
		return null;
	}

	return (
		<select
			className={clsx('navbar__item navbar__link', className)}
			aria-label="Preferred code language"
			value={value}
			onChange={(event) => setValue(event.target.value)}
		>
			{LANGUAGE_OPTIONS.map((option) => (
				<option key={option.value} value={option.value}>
					{option.label}
				</option>
			))}
		</select>
	);
}
